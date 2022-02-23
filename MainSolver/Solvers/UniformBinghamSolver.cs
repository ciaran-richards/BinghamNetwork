﻿using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MainSolver.Solvers
{
    public class UniformBinghamSolver
    {
        //Regularising Constant for iterative solver  
        private readonly double reg = Math.Pow(10, -5);

        public Network Solve(Network net)
        {
            double H = net.GradPressure * Math.Cos(net.PressAngle) * net.Length;
            double V = net.GradPressure * Math.Sin(net.PressAngle) * net.Length;
            int N = net.Nodes;
            var invN1 = 1d / (net.Nodes-1);

            var ran = new Random();
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
            var correction = new DenseVector((N - 1) * (N - 1) - 1);
            correction = (DenseVector)resVec.Add(1000);
            
            int iteration = 0;


            //while (correction.Max(x => Math.Abs(x)) / (H + V) > Math.Pow(10, -5))
            while (net.MaxResidual > Math.Pow(10,-3) || iteration<1)
            {
                var hb = HBinghamGrad(net);
                var vb = VBinghamGrad(net);
                CalculateFlows(ref net, hb, vb);
                net.CalculateResiduals();
                resVec = ResidualVector(net);
                correction = PressureCorrection(net, hb, vb);
                ApplyCorrection(ref net, correction);
                iteration++;
            }

            return net;
        }


        double[][] HBinghamGrad(Network net)
        {
            var N = net.Nodes;
            var bGrad = HorizontalArray(N);

            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    bGrad[i][j] = -(net.pressure[i + 1][j] - net.pressure[i][j]) *0.5 * net.inv_hLength[i][j] * net.Inv_Yield;
                }
            }

            return bGrad;
        }

        double[][] VBinghamGrad(Network net)
        {
            var N = net.Nodes;
            var bGrad = VerticalArray(N);

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    bGrad[i][j] = -(net.pressure[i][j + 1] - net.pressure[i][j]) * 0.5 * net.inv_vLength[i][j] * net.Inv_Yield; 
                }
            }
            return bGrad;
        }

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

        void CalculateFlows(ref Network net, double[][] hBingham, double[][] vBingham)
        {
            var N = net.Nodes;
            //Calculate horizontal flow
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    net.hFlow[i][j] = FlowRate(hBingham[i][j]);
                }
            }

            //Calculate vertical flow
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    net.vFlow[i][j] = FlowRate(vBingham[i][j]);
                }
            }

        }

        double FlowRate(double binghamGradient)
        {
            var abs = Math.Abs(binghamGradient);
            var invAbs = 1 / abs;
            double planeFlowFunc;

            if (abs >= 1)
            {
                planeFlowFunc = 1 - (1.5 - reg) * invAbs + 0.5 * Math.Pow(invAbs, 3);
            }
            else
            {
                planeFlowFunc = reg;
            }

            return binghamGradient * planeFlowFunc;
        }

        double FlowDerivative(double binghamGradient)
        {
            //Calculates the derivative of the channel flow with respect to the binghamGradient
            var abs = Math.Abs(binghamGradient);
            var invAbs = 1 / abs;
            var deriv = 0d;
            if (abs > 1)
            {
                var debug = 1d - Math.Pow(invAbs, 3);
                deriv = 1d - Math.Pow(invAbs, 3);
            }

            return Math.Max(deriv, reg);
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

        //Check residuals

        //While residuals are above threshold:

        DenseVector PressureCorrection(Network net, double[][] hB, double[][] vB)
        {
            var N = net.Nodes;
            var M = (N - 1) * (N - 1) - 1;
            double[][] rowArrays = new double[M][];
            int u;
            var invYi = net.Inv_Yield;
            double diag;
            var yield = net.YieldPressure;
            //Calculate the Jacobian Matrix
            //Different order to Newtonian Matrix
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
                            //Derivative wrt RH node Pressure - Except Right Bound
                            row[u + 1] = FlowDerivative(hB[p][q]) * net.inv_hLength[p][q] * 2 * yield; //*0.5;
                            //row[u + 1] = net.inv_hLength[p][q];
                        }
                        
                        if (p == 0 && q > 0)
                        {
                            //Derivative wrt RH node Pressure - Right Bound
                            row[u + (N - 2)] = FlowDerivative(hB[N - 2][q]) * net.inv_hLength[N - 2][q] * 2 * yield; //*0.5;
                            //row[u + (N - 2)] = net.inv_hLength[N - 2][q];
                        }

                        if (u + N - 1 < M)
                        {  //Derivative wrt Upper node Pressure - Except Top Bound
                            row[u + N - 1] = FlowDerivative(vB[p][q]) * net.inv_vLength[p][q] * 2 * yield; //*0.5;
                            //row[u + N - 1] = net.inv_vLength[p][q];
                        }

                        if (u + (N - 2) * (N - 1) < M)
                        {
                            //Derivative wrt top node pressure on it's column - Bottom Bound only
                            row[u + (N - 2) * (N - 1)] = FlowDerivative(vB[p][N - 2]) * net.inv_vLength[p][N - 2] * 2 * yield; //*0.5; //Bottom
                            //row[u + (N - 2) * (N - 1)] = net.inv_vLength[p][N - 2];
                        }
                        rowArrays[u] = row;
                    }
                }
            }

            var Jacobian = DenseMatrix.OfRowArrays(rowArrays);
            var lowerAT = Jacobian.Transpose().StrictlyLowerTriangle();
            Jacobian = (DenseMatrix)Jacobian.Add(lowerAT);

            //Calculate the diagonal terms 
            for (int k = 0; k < M; k++)
            {
                Jacobian[k, k] = -Jacobian.Row(k).Sum();
            }

            //Add boundary conditions four diagonal terms
            Jacobian[0, 0] = Jacobian[0, 0] - FlowDerivative(hB[0][0]) * net.inv_hLength[0][0] * 2 * yield;
            Jacobian[N-2, N-2] = Jacobian[N-2, N-2] - FlowDerivative(vB[0][0]) * net.inv_vLength[0][0] * 2 * yield;
            Jacobian[N-3, N-3] = Jacobian[N-3, N-3] - FlowDerivative(hB[N-2][0]) * net.inv_hLength[N-2][0] * 2 * yield;
            u = (N - 1) * (N - 2) - 1;
            Jacobian[u,u] = Jacobian[u,u] - FlowDerivative(vB[0][N-2]) * net.inv_vLength[0][N-2] * 2 * yield;



            var debug = Jacobian.EnumerateRows();

            var resVec = ResidualVector(net);

            var maxRes = resVec.Max(x => Math.Abs(x));

            var det = Jacobian.Determinant();

            var invJac = Jacobian.Inverse();

            var pressureCorrect = (DenseVector)invJac.Multiply(-resVec);

            //var normVec = pressureCorrect.Normalize(2);

            return (DenseVector)pressureCorrect;
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


        Vector NewPressures; //Pressure Vec 

        //Calculate Flow

        //Check residuals

        //Finally, apply to network object.

    }
}