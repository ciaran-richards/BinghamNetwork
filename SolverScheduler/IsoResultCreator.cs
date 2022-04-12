using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainSolver;
using MainSolver.Solvers;

namespace SolverScheduler
{
    class IsoResultCreator
    {

        private NetworkCreator NetworkCreator;

        private const double Deg = 180 / Math.PI;
        private const double Rad = Math.PI / 180;
        private const int MaxAngle = 45;

        public List<IsoResultStruct> EvaluateAngleRange(Network[] Networks, double pGrad)
        {
            
            double angle;
            var ResultRange = new List<IsoResultStruct>(MaxAngle+1);
            for (double i = MaxAngle; i >= 0; i = i-45)
            {
                angle = i * Rad;
                ResultRange.Add(Evaluate(Networks, pGrad, angle));
            }

            return ResultRange;
        }

        public IsoResultStruct Evaluate(Network[] Networks, double pGrad, double pAngle)
        {
            var BinghamSolver = new BinghamSolver();
            var NewtonSolver = new NewtonianSolver();
            int Count = Networks.Length;
            //If Flow is Zero, the Angle is Not A Number, exclude from all processing. 
            double YieldPressure = 0.5;
            int FlowSamples = 0;
            int AngleSamples = 0;

            double[] flowArray = new double[Count];
            double[] angleDeltaArray = new double[Count];
            double SumFlow = 0;
            double SumAngleDelta = 0;

            Network newtonNet;
            Network binghamNet;

            for (int i = 0; i < Count; i++)
            {
                Networks[i].YieldPressure = YieldPressure;
                Networks[i].GradPressure = pGrad;
                Networks[i].PressAngle = pAngle;
                newtonNet = NewtonSolver.Solve(Networks[i].Copy());
                binghamNet = BinghamSolver.Solve(Networks[i].Copy());

                if (binghamNet != null)
                {
                    flowArray[i] = binghamNet.FlowRate / newtonNet.FlowRate;
                    SumFlow += flowArray[i];
                    FlowSamples += 1;

                    angleDeltaArray[i] = (binghamNet.FlowAngle - newtonNet.FlowAngle);
                    if (!double.IsNaN(binghamNet.FlowAngle))
                    {
                        SumAngleDelta += (binghamNet.FlowAngle - newtonNet.FlowAngle);
                        AngleSamples += 1;
                    }

                    Console.WriteLine($@"{i} +  {Math.Round(Networks[i].PressAngle * 180 / 3.14, 1)}");
                }
                else
                {
                    Console.WriteLine($@"{i} +  {Math.Round(Networks[i].PressAngle * 180 / 3.14, 1)} - Skipped");
                    flowArray[i] = Double.NaN;
                    angleDeltaArray[i] = Double.NaN;
                }
            }

            //Flow Mean and Standard Deviation
            double flowMean = SumFlow / FlowSamples;
            double flowSumSquares = 0;
            for (int i = 0; i < Count; i++)
            {
                if (!double.IsNaN(flowArray[i]))
                {
                    flowSumSquares += (flowArray[i] - flowMean) * (flowArray[i] - flowMean);
                }
            }
            double flowSD = Math.Sqrt(flowSumSquares / (FlowSamples - 1));

            double angleMean = 0;
            if (AngleSamples > 0)
            {
                angleMean = SumAngleDelta / AngleSamples;
            }
            double angleSD = 0;
            double angleSumSquares = 0;

            for (int i = 0; i < AngleSamples; i++)
            {
                if (!double.IsNaN(angleDeltaArray[i]))
                {
                    angleSumSquares += (angleDeltaArray[i] - angleMean) * (angleDeltaArray[i] - angleMean);
                }
            }
            
            if (AngleSamples > 1)
            {
                angleSD = Math.Sqrt(angleSumSquares / (AngleSamples - 1));
            }
            
            var result = new IsoResultStruct();
            result.BinghamGradient = pGrad / (2 * YieldPressure);
            result.BinghamGradAngle = pAngle * Deg;
            result.FlowRatioMean = flowMean;
            result.FlowRatioSD = flowSD;
            result.FlowAngleDeltaMean = angleMean*Deg;
            result.FlowAngleDeltaSD = angleSD*Deg;

            return result;
        }



    }
}
