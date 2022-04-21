using System;
using System.Linq;
using System.Text;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MainSolver
{
    public class NewtonianSolver
    {
        public Network Solve(Network net)
        {
            double H = net.GradPressure * Math.Cos(net.PressAngle);
            double V = net.GradPressure * Math.Sin(net.PressAngle);

            var pVector = PressureVector(net);
            Control.UseNativeMKL();

            int N = net.Nodes;
            int M = (N - 1) * (N - 1) - 1; //Matrix + Vector Dimension
            int u;
            net.pressure[0][0] = H + V;
            for (int q = 0; q < N - 1; q++)
            {
                for (int p = 0; p < N - 1; p++)
                {
                    u = (p - 1) + (N - 1) * q; //Linearised Index
                    if (u >= 0)
                    {
                        net.pressure[p][q] = pVector[u];
                    }
                }
            }
            for (int k = 0; k < N; k++)
            {
                net.pressure[N - 1][k] = net.pressure[0][k] - H;
                net.pressure[k][N-1] = net.pressure[k][0] - V;
            }

            var hCoef = HChannelCoef(net);
            var vCoef = VChannelCoef(net);

            //Calculate horizontal flow
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    net.hFlow[i][j] = -(net.pressure[i + 1][j] - net.pressure[i][j]) * hCoef[i][j] / 12;
                }
            } 
            //Calculate vertical flow
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    net.vFlow[i][j] = -(net.pressure[i][j+1] - net.pressure[i][j]) * vCoef[i][j] / 12;
                }
            }
            
            net.CalculateResiduals();
            net.CalculateBulkFlow();

            var Bulk = net.FlowRate;
            var FlAng = net.FlowAngle*180/Math.PI;
            var PressAngle = Math.Atan(V / H)*180/Math.PI;

            return net;
        }

        Vector<double> PressureVector(Network net)
        {
            int N = net.Nodes;
            int M = (N - 1) * (N - 1) - 1; //Matrix + Vector Dimension

            //Solve AP = Bv+Ch
            //With Matrix A and vectors X, B and C, of dimension N
            //Scalar v and h being the pressure components.

            DenseMatrix A;
            var B = new DenseVector(M);
            var C = new DenseVector(M);
            int u;

            double[][] rowArrays = new double[M][];
            double[] row;
            double diag;

            var hCoef = HChannelCoef(net);
            var vCoef = VChannelCoef(net);


            for (int q = 0; q < N - 1; q++)
            {
                for (int p = 0; p < N - 1; p++)
                {
                    u = (p - 1) + (N - 1) * q ; //Linearised Index
                    if (u >= 0)
                    {
                        diag = 0;
                        row = new double[M];
                       //Diagonal Component Calculation
                        diag -= (hCoef[p][q] + vCoef[p][q]);
                        if (p > 0)
                            diag -= (hCoef[p - 1][q]); //Except Left Boundary
                        else
                            diag -= (hCoef[N - 2][q]); //Left
                        if (q > 0)
                            diag -= (vCoef[p][q - 1]); //Except Bottom
                        else
                            diag -= (vCoef[p][N - 2]); //Bottom
                        row[u] = diag;

                        //Other row components, Top half only
                        if (p < N - 2)
                            row[u + 1] = hCoef[p][q]; //Except Right Bound
                        if (p == 0 && q > 0)
                            row[u + (N - 2)] = hCoef[N - 2][q]; //Right
                        if (u + N - 1 < M)
                            row[u + N - 1] = vCoef[p][q]; //Except Top
                        if (u + (N - 2) * (N - 1) < M)
                            row[u + (N - 2) * (N - 1)] = vCoef[p][N - 2]; //Bottom
                        rowArrays[u] = row;

                        //Vector B and C Calculations
                        
                        if (p == 0) //Left
                        {
                            B[u] -= hCoef[N - 2][q];
                            if (q == 1)
                            {
                                B[u] -= vCoef[0][0];
                                C[u] -= vCoef[0][0]; //Left Bottom
                            }
                            if (q == N - 2)
                            {
                                B[u] -= vCoef[0][N - 2]; //Left Top
                            }
                        }
                        if (q == N - 2 && p > 0)
                            C[u] += vCoef[p][q]; //Top but Exclude Left


                        if (q == 0) //Bottom
                        {
                            C[u] -= vCoef[p][N - 2];
                            if (p == 1)
                            {
                                B[u] -= hCoef[0][0];
                                C[u] -= hCoef[0][0]; //Bottom Left
                            }
                            if (p == N - 2)
                            {
                                C[u] -= hCoef[N - 2][0]; //Bottom Right
                            }
                        }
                        if (p == N - 2 && q > 0)
                            B[u] += hCoef[p][q]; //Right but Exclude Bottom

                    }
                }
            }
            A = DenseMatrix.OfRowArrays(rowArrays);
            
            //Only compute top half.
            var lowerAT = A.Transpose().StrictlyLowerTriangle();
            A = (DenseMatrix)A.Add(lowerAT);

            double H = net.GradPressure * Math.Cos(net.PressAngle);
            double V = net.GradPressure * Math.Sin(net.PressAngle);

            var pressureVec = A.Solve(H * B + V * C);

            return pressureVec;
        }

        private double[][] HChannelCoef(Network net)
        {
            int u;
            int N = net.Nodes;
            var hCoef = new double[N - 1][];

            for (int i = 0; i < N - 1; i++)
            {
                hCoef[i] = new double[N];
            }

            //Calculate horizonal lengths
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    hCoef[i][j] = net.inv_hLength[i][j]*Math.Pow(net.hWidth[i][j],3);
                }
            }

            return hCoef;
        }

        private double[][] VChannelCoef(Network net)
        {
            int u;
            int N = net.Nodes;
            double[][] vCoef = new double[N][];
            for (int i = 0; i <= N - 1; i++)
            {
                vCoef[i] = new double[N - 1];
            }

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    vCoef[i][j] = net.inv_vLength[i][j] * Math.Pow(net.vWidth[i][j], 3);
                }
            }

            return vCoef;
        }
    }
}
