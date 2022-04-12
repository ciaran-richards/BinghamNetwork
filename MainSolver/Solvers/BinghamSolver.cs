using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Providers.SparseSolver.Mkl;

namespace MainSolver.Solvers
{
    public class BinghamSolver : IterativeSolver
    {
        public override double FlowRate(double pGrad, double width, double yield)
        {
            var binghamGrad = pGrad * width * 0.5 / yield;
            return NDFlowRate(binghamGrad) * 2 * yield * Math.Pow(width, 2);
        }

        double NDFlowRate(double binghamGradient)
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

        public override double FlowDerivative(double pGrad, double width, double invLength, double yield)
        {
            var binghamGrad = pGrad * width * 0.5 / yield;
            var derivative = NDFlowDerivative(binghamGrad) * Math.Pow(width, 3) * invLength;
            //return derivative;

            //Now with Finite Difference

            var delta = Math.Pow(10, -5);
            var flow0 = FlowRate(pGrad, width, yield);
            var flow1 = FlowRate(pGrad + delta, width, yield);
            var fdDerivative = (flow1 - flow0) / delta;
            fdDerivative = Math.Max(fdDerivative, MinDerivative);
            fdDerivative = fdDerivative * invLength;
            return fdDerivative;
        }

        double NDFlowDerivative(double binghamGradient)
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

            return Math.Max(deriv, MinDerivative);
        }
    }
}
