using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainSolver.Solvers
{ 
    public interface IIterativeSolver
    {
        double FlowRate(double pGrad, double width, double yield);
        double FlowDerivative(double pGrad, double width, double invLength, double yield);

    }
}
