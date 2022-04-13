using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainSolver.Solvers
{
    public class HerschelBulkleySolver:IterativeSolver
    {
        public override double FlowRate(double pGrad, double width, double yield, double index)
        {
            var dpMag = Math.Abs(pGrad);
            var sgn = Math.Sign(pGrad);
            var q1 = Math.Pow(width / 2, 2 * index + 1);
            var q2 = Math.Pow(dpMag * q1, 1 / index);
            var shearIndexFlow =  sgn * q2 * 2 * index / (2 * index + 1);

            var binghamGrad = pGrad * width * 0.5 / yield;
            var flow = shearIndexFlow * PlaneFlowFunction(binghamGrad, index);

            if (double.IsNaN(flow))
            {
                return 0;
            }

            return flow;
        }

        private double PlaneFlowFunction(double bingGrad, double index)
        {
            var bGradAbs = Math.Abs(bingGrad);
            if (bGradAbs < 1)
            {
                return reg * bGradAbs;
            }
            var planFunc = (1 + index / ((index + 1) * bGradAbs)) * Math.Pow((1 - 1 / bGradAbs), 1 + 1 / index) + reg;
            return planFunc;
        }

        public override double FlowDerivative(double pGrad, double width, double invLength, double yield, double index)
        {
            var delta = Math.Pow(10, -5);
            var flow0 = FlowRate(pGrad, width, yield, index);
            var flow1 = FlowRate(pGrad + delta, width, yield, index);
            var fdDerivative = (flow1 - flow0) / delta;
            fdDerivative = Math.Max(fdDerivative, MinDerivative);
            fdDerivative = fdDerivative * invLength;

            if (double.IsNaN(fdDerivative))
            {
                return 0;
            }

            return fdDerivative;
        }
    }
}
