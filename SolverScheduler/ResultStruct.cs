using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverScheduler
{ 
    struct ResultStruct
    {
        public double BinghamGradient { get; set; }
        public double BinghamGradAngle { get; set; }
        public double FlowRatioMean { get; set; }
        public double FlowRatioSD { get; set; }
        public double FlowAngleMean { get; set; }
        public double FlowAngleSD { get; set; }
    }
    
}
