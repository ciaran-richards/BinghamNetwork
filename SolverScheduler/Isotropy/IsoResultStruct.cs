using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverScheduler
{ 
    public struct IsoResultStruct
    {
        public double BinghamGradient { get; set; }
        public double BinghamGradAngle { get; set; }
        public double ShearIndex { get; set; }
        public double FlowRatioMean { get; set; }
        public double FlowRatioSD { get; set; }
        public double FlowAngleDeltaMean { get; set; }
        public double FlowAngleDeltaSD { get; set; }
    }
    
}
