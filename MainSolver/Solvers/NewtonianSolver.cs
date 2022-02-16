﻿using System;
using System.Linq;
using System.Text;
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

            //Calculate horizontal flow
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    net.hFlow[i][j] = -(net.pressure[i + 1][j] - net.pressure[i][j]) * net.inv_hLength[i][j];
                }
            } 
            //Calculate vertical flow
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    net.vFlow[i][j] = -(net.pressure[i][j+1] - net.pressure[i][j]) * net.inv_vLength[i][j];
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

            SparseMatrix A;
            var B = new DenseVector(M);
            var C = new DenseVector(M);
            int u;

            double[][] rowArrays = new double[M][];
            double[] row;
            double diag;

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
                        diag -= (net.inv_hLength[p][q] + net.inv_vLength[p][q]);
                        if (p > 0)
                            diag -= (net.inv_hLength[p - 1][q]); //Except Left Boundary
                        else
                            diag -= (net.inv_hLength[N - 2][q]); //Left
                        if (q > 0)
                            diag -= (net.inv_vLength[p][q - 1]); //Except Bottom
                        else
                            diag -= (net.inv_vLength[p][N - 2]); //Bottom
                        row[u] = diag;

                        //Other row components, Top half only
                        if (p < N - 2)
                            row[u + 1] = net.inv_hLength[p][q]; //Except Right Bound
                        if (p == 0 && q > 0)
                            row[u + (N - 2)] = net.inv_hLength[N - 2][q]; //Right
                        if (u + N - 1 < M)
                            row[u + N - 1] = net.inv_vLength[p][q]; //Except Top
                        if (u + (N - 2) * (N - 1) < M)
                            row[u + (N - 2) * (N - 1)] = net.inv_vLength[p][N - 2]; //Bottom
                        rowArrays[u] = row;

                        //Vector B and C Calculations
                        
                        if (p == 0) //Left
                        {
                            B[u] -= net.inv_hLength[N - 2][q];
                            if (q == 1)
                            {
                                B[u] -= net.inv_vLength[0][0];
                                C[u] -= net.inv_vLength[0][0]; //Left Bottom
                            }
                            if (q == N - 2)
                            {
                                B[u] -= net.inv_vLength[0][N - 2]; //Left Top
                            }
                        }
                        if (q == N - 2 && p > 0)
                            C[u] += net.inv_vLength[p][q]; //Top but Exclude Left


                        if (q == 0) //Bottom
                        {
                            C[u] -= net.inv_vLength[p][N - 2];
                            if (p == 1)
                            {
                                B[u] -= net.inv_hLength[0][0];
                                C[u] -= net.inv_hLength[0][0]; //Bottom Left
                            }
                            if (p == N - 2)
                            {
                                C[u] -= net.inv_hLength[N - 2][0]; //Bottom Right
                            }
                        }
                        if (p == N - 2 && q > 0)
                            B[u] += net.inv_hLength[p][q]; //Right but Exclude Bottom

                    }
                }
            }
            A = SparseMatrix.OfRowArrays(rowArrays);
            
            //Only compute top half.
            var lowerAT = A.Transpose().StrictlyLowerTriangle();
            A = (SparseMatrix)A.Add(lowerAT);

            double H = net.GradPressure * Math.Cos(net.PressAngle);
            double V = net.GradPressure * Math.Sin(net.PressAngle);

            var pressureVec = A.Inverse().Multiply(H * B + V * C);

            return pressureVec;
        }

    }
}
