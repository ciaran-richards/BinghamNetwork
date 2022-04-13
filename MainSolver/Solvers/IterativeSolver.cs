using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Providers.SparseSolver.Mkl;

namespace MainSolver.Solvers
{
    public abstract class IterativeSolver : IIterativeSolver
    {
        private DenseMatrix Jacobian;
        //Regularising Constant for iterative solver  
        public double reg = Math.Pow(10, -5);
        public double MaxRedidual = Math.Pow(10, -4);
        public double MinDerivative = Math.Pow(10, -3);
        public double MaxIterations = 30;

        public Network Solve(Network net)
        {
            //returns Null if it does not converge
            double H = net.GradPressure * Math.Cos(net.PressAngle) * net.Length;
            double V = net.GradPressure * Math.Sin(net.PressAngle) * net.Length;
            int N = net.Nodes;
            var invN1 = 1d / (net.Nodes-1);
            Control.UseNativeMKL();

            //Square Network Pressure Field as first estimate.
            for (int i = 0; i < N-1; i++)
            {
                for (int j = 0; j < N-1; j++)
                {
                    net.pressure[i][j] = H *(N-1-i)* invN1;
                    net.pressure[i][j] += V * (N-1-j)* invN1;
                }
            }
            net.pressure[0][0] = H + V;

            for (int k = 0; k < N; k++)
            {
                net.pressure[N - 1][k] = net.pressure[0][k] - H;
                net.pressure[k][N - 1] = net.pressure[k][0] - V;
            }

            var resVec = new DenseVector((N-1)*(N-1)-1);
            DenseVector correction;
            int iteration = 0;

            while (net.MaxResidual > MaxRedidual || iteration<1)
            {
                var hpGrad = HPressureGrad(net);
                var vpGrad = VPressureGrad(net);
                CalculateFlows(ref net, hpGrad, vpGrad);
                net.CalculateResiduals();
                resVec = ResidualVector(net);
                correction = PressureCorrection(net, hpGrad, vpGrad);
                ApplyCorrection(ref net, correction);
                iteration++;
                var debug = net.PressAngle * 180 / 3.14;

                if (iteration > MaxIterations)
                {
                    return null;
                }
                //Console.WriteLine(iteration);
                //Console.WriteLine("Max:" + net.MaxResidual);
                //Console.WriteLine("Ave:" + net.AveResidual);
            }
            net.CalculateBulkFlow();

            return net;
        }

        DenseVector PressureCorrection(Network net, double[][] hdP, double[][] vdP)
        {
            var N = net.Nodes;
            var M = (N - 1) * (N - 1) - 1;
            double[][] rowArrays = new double[M][];
            int u;
            var yield = net.YieldPressure;
            //To calculate the Jacobian Matrix
            //Calculate row-by-row the top-right side of the matrix, but without the diagonals
            //Create the full matrix with external library
            //The diagonal term is then the negative sum of the other terms on its row 

            double[] row;
            for (int q = 0; q < N - 1; q++)
            {
                for (int p = 0; p < N - 1; p++)
                {
                    u = (p - 1) + (N - 1) * q; //Linearised Index
                    if (u >= 0)
                    {
                        row = new double[M];

                        //Row components - Top half only
                        if (p < N - 2)
                        {
                            //Derivative wrt RH node Pressure - Except Right Bound.
                            row[u + 1] = 
                                FlowDerivative(hdP[p][q], net.hWidth[p][q], net.inv_hLength[p][q], yield, net.ShearIndex);

                        }

                        if (p == 0 && q > 0)
                        {
                            //Derivative wrt RH node Pressure - Right Bound
                            row[u + (N - 2)] = 
                                FlowDerivative(hdP[N - 2][q], net.hWidth[N - 2][q], net.inv_hLength[N - 2][q], yield, net.ShearIndex);
                        }

                        if (u + N - 1 < M)
                        {  //Derivative wrt Upper node Pressure - Except Top Bound
                            row[u + N - 1] = 
                                FlowDerivative(vdP[p][q], net.vWidth[p][q], net.inv_vLength[p][q], yield, net.ShearIndex);
                        }

                        if (u + (N - 2) * (N - 1) < M)
                        {
                            //Derivative wrt top node pressure on it's column - Bottom Bound only
                            row[u + (N - 2) * (N - 1)] =
                                FlowDerivative(vdP[p][N - 2], net.vWidth[p][N - 2], net.inv_vLength[p][N - 2], yield, net.ShearIndex);
                        }
                        rowArrays[u] = row;
                    }
                }
            }

            Jacobian = DenseMatrix.OfRowArrays(rowArrays);
            var lowerAT = Jacobian.Transpose().StrictlyLowerTriangle();
            Jacobian = (DenseMatrix)Jacobian.Add(lowerAT);

            //Calculate the diagonal terms 
            for (int k = 0; k < M; k++)
            {
                Jacobian[k, k] = -Jacobian.Row(k).Sum();
            }

            //Add boundary conditions four diagonal terms
            Jacobian[0, 0] = Jacobian[0, 0] - 
                             FlowDerivative(hdP[0][0], net.hWidth[0][0], net.inv_hLength[0][0], yield, net.ShearIndex);

            Jacobian[N - 2, N - 2] = Jacobian[N - 2, N - 2] -
                                     FlowDerivative(vdP[0][0], net.vWidth[0][0], net.inv_vLength[0][0], yield, net.ShearIndex);

            Jacobian[N - 3, N - 3] = Jacobian[N - 3, N - 3] -
                                     FlowDerivative(hdP[N - 2][0], net.hWidth[N - 2][0], net.inv_hLength[N - 2][0], yield, net.ShearIndex);

            u = (N - 1) * (N - 2) - 1;
            Jacobian[u, u] = Jacobian[u, u] -
                             FlowDerivative(vdP[0][N - 2], net.vWidth[0][N - 2], net.inv_hLength[0][N - 2], yield, net.ShearIndex);
            

            var resVec = ResidualVector(net);


            var pressureCorrect = Jacobian.Solve(-resVec);

            return (DenseVector)pressureCorrect;
        }

        public void CalculateFlows(ref Network net, double[][] hPGrad, double[][] vPGrad)
        {
            var N = net.Nodes;
            //Calculate horizontal flow
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    net.h_Blocked[i][j] = hPGrad[i][j]*net.hWidth[i][j]*0.5*net.Inv_Yield<1;
                    net.hFlow[i][j] = FlowRate(hPGrad[i][j], net.hWidth[i][j], net.YieldPressure, net.ShearIndex);
                }
            }

            //Calculate vertical flow
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    net.v_Blocked[i][j] = vPGrad[i][j]*net.vWidth[i][j]*0.5*net.Inv_Yield<1;
                    net.vFlow[i][j] = FlowRate(vPGrad[i][j], net.vWidth[i][j], net.YieldPressure, net.ShearIndex);
                }
            }

        }

        public abstract double FlowRate(double pGrad, double width, double yield, double index);

        public abstract double FlowDerivative(double pGrad, double width, double invLength, double yield, double index);

        //Helper Methods Below

        double[][] VerticalArray(int N)
        {
            var verticalArray = new double[N][];
            for (int i = 0; i < N; i++)
            {
                verticalArray[i] = new double[N - 1];
            }

            return verticalArray;
        }

        double[][] HorizontalArray(int N)
        {
            var horizontalArray = new double[N - 1][];
            for (int i = 0; i < N - 1; i++)
            {
                horizontalArray[i] = new double[N];
            }

            return horizontalArray;
        }

        double[][] HPressureGrad(Network net)
        {
            var N = net.Nodes;
            var pGrad = HorizontalArray(N);

            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    pGrad[i][j] = -(net.pressure[i + 1][j] - net.pressure[i][j]) * net.inv_hLength[i][j];
                }
            }

            return pGrad;
        }

        double[][] VPressureGrad(Network net)
        {
            var N = net.Nodes;
            var pGrad = VerticalArray(N);

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    pGrad[i][j] = -(net.pressure[i][j + 1] - net.pressure[i][j]) * net.inv_vLength[i][j];
                }
            }
            return pGrad;
        }

        DenseVector ResidualVector(Network net)
        {
            var residual = net.residual;
            var N = net.Nodes;
            var resVec = new DenseVector((N - 1) * (N - 1) - 1);
            int u; //Linear Index
            for (int q = 0; q < N - 1; q++)
            {
                for (int p = 0; p < N - 1; p++)
                {
                    u = (p - 1) + (N - 1) * q; //Linearised Index
                    if (u >= 0)
                    {
                        resVec[u] = net.residual[p][q];
                    }
                }
            }
            return resVec;
        }

        private void ApplyCorrection(ref Network net, DenseVector correction)
        {
            int u;
            var N = net.Nodes;

            double H = net.GradPressure * Math.Cos(net.PressAngle) * net.Length;
            double V = net.GradPressure * Math.Sin(net.PressAngle) * net.Length;

            for (int q = 0; q < N - 1; q++)
            {
                for (int p = 0; p < N - 1; p++)
                {
                    u = (p - 1) + (N - 1) * q; //Linearised Index
                    if (u >= 0)
                    {
                        net.pressure[p][q] += correction[u];
                    }
                }
            }
            for (int k = 0; k < N; k++)
            {
                net.pressure[N - 1][k] = net.pressure[0][k] - H;
                net.pressure[k][N - 1] = net.pressure[k][0] - V;
            }
        }


    }
}
