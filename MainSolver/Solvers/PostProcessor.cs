using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MainSolver.Solvers
{
    public class PostProcessor
    {
        public Network PostProcess(Network net)
        {
            var hBingham = HBinghamGrad(net);
            var vBingham = VBinghamGrad(net);

            var N = net.Nodes;
            //Calculate horizontal flow
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (Math.Abs(hBingham[i][j]) < 1d)
                    {
                        net.hFlow[i][j] = 0;
                    }
                }
            }

            //Calculate vertical flow
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    if (Math.Abs(vBingham[i][j]) < 1)
                    {
                        net.vFlow[i][j] = 0;
                    }
                }
            }

            //Remove dead ends from central nodes.

            bool north;
            bool east;
            bool west;
            bool south;

            for (int k = 0; k < 2 * N - 6; k++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    for (int j = 1; j < N - 1; j++)
                    {
                        north = (net.vFlow[i][j] != 0);
                        south = (net.vFlow[i][j - 1] != 0);
                        east = (net.hFlow[i][j] != 0);
                        west = (net.hFlow[i - 1][j] != 0);

                        //If only one channel into a central node has flow, then continuity
                        //is violated, the flowing channel must be halted.

                        if (north && !south && !east && !west)
                            net.vFlow[i][j] = 0;

                        if (!north && south && !east && !west)
                            net.vFlow[i][j - 1] = 0;

                        if (!north && !south && east && !west)
                            net.hFlow[i][j] = 0;

                        if (!north && !south && !east && west)
                            net.hFlow[i - 1][j] = 0;
                    }
                }
            }

            //If any column of horizontal channels AND any row of vertical channels is
            //blocked then no flow is in the network and all channels are set to zero.

            //Indexing method allows LINQ to determine horizontal flow
            var isHorizontalFlow = !net.hFlow.Any(x => x.All(y => y == 0));
            
            //More complex for vertical flow
            bool isVerticalFlow = true;
            bool isColumnFlow = false;
            for (int j = 0; j < N - 1; j++)
            {
                for (int i = 0; i < N; i++)
                {
                    isColumnFlow = false;
                    if (net.vFlow[i][j] != 0)
                        isColumnFlow = true;

                }
                //If any row of vertical channels is completely blocked then there is no vertical flow
                if (!isColumnFlow)
                    isVerticalFlow = false;
            }

            if (!isVerticalFlow && !isHorizontalFlow)
            {
                
                net.hFlow = HorizontalArray(N);
                net.vFlow = VerticalArray(N);
            }

            net.CalculateResiduals();
            net.CalculateBulkFlow();
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
                    bGrad[i][j] = -(net.pressure[i + 1][j] - net.pressure[i][j]) * 0.5 * net.inv_hLength[i][j] * net.Inv_Yield;
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

    }
}
