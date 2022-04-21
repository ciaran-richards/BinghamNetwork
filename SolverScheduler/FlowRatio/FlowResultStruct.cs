using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverScheduler
{
    public struct FlowResultStruct
    {
        public double BinghamGrad { get; set; }

        public double ShearIndex { get; set; }
        public double FlowRatio { get; set; }
        public double FlowDiscrepancy { get; set; }
    }
}
