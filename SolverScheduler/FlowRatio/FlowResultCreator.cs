using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainSolver;
using MainSolver.Solvers;

namespace SolverScheduler
{
    class FlowResultCreator
    {

        private NetworkCreator NetworkCreator;

        private const double Deg = 180 / Math.PI;
        private const double Rad = Math.PI / 180;
        private const int MaxAngle = 45;
        private readonly double InvLog2 = Math.Log10(2);

        public FlowResultStruct EvaluateFlow(Network[] rough, Network[] medium, Network[] fine, double pGrad, double index)
        {

            var uMf = EvaluateAngle(fine, pGrad, MaxAngle, index);

            var result = new FlowResultStruct();
            result.BinghamGrad = pGrad;
            result.ShearIndex = index;
            result.FlowRatio = uMf;
            result.FlowDiscrepancy = 0;//uMf - u0Inf;

            Console.WriteLine($@"{pGrad} + {result.FlowRatio} + {result.FlowDiscrepancy}");
            return result;
        }

        public double EvaluateAngle(Network[] Networks, double pGrad, double pAngle, double index)
        {
            var HBSolver = new HerschelBulkleySolver();
            var BinghamSolver = new BinghamSolver();
            var SIndSolver = new ShearIndexSolver();
            var NewtonSolver = new NewtonianSolver();
            int Count = Networks.Length;
            //If Flow is Zero, the Angle is Not A Number, exclude from all processing. 
            double YieldPressure = 0.5;
            int FlowSamples = 0;
            int AngleSamples = 0;

            double[] flowArray = new double[Count];
            double SumFlow = 0;

            Network newtonNet;
            Network hbNet;
            Network sindNet;

            for (int i = 0; i < Count; i++)
            {
                Networks[i].YieldPressure = YieldPressure;
                Networks[i].GradPressure = pGrad;
                Networks[i].ShearIndex = index;
                Networks[i].PressAngle = pAngle*Rad;
                newtonNet = NewtonSolver.Solve(Networks[i].Copy());
                hbNet = HBSolver.Solve(Networks[i].Copy());

                if (Math.Abs(index - 1) < 0.000000001)
                {
                    sindNet = newtonNet;
                }
                else
                {
                    sindNet = SIndSolver.Solve(Networks[i].Copy());
                }

                if (hbNet != null && sindNet != null)
                {
                    flowArray[i] = hbNet.FlowRate / sindNet.FlowRate;
                    SumFlow += flowArray[i];
                    FlowSamples += 1;
                    Console.WriteLine($@"{i} +  {pAngle}");
                }
                else
                {
                    flowArray[i] = Double.NaN;
                    Console.WriteLine($@"{i} +  {Math.Round(Networks[i].PressAngle * 180 / 3.14, 1)} - Skipped");
                }
            }

            //Flow Mean and Standard Deviation
            double flowMean = SumFlow / FlowSamples;

            return flowMean;
        }

       


    }
}
