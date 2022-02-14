using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainSolver
{
    public class CreatorSettings
    {
        public string Name;
        public int Nodes;
        public double Bingham;
        public double Length;
        public Distro DisplacementDistro;
        private double displacementLimit;
        public double DisplacementLimit
        {
            get { return displacementLimit;}
            set
            {
                if ((value < 0) || (value > 1))
                    displacementLimit = 0;
                else
                    displacementLimit = value;
            }
        }

        public Distro TaperDistro;
        private double taperLimit;
        public double TaperLimit
        {
            get { return taperLimit; }
            set
            {
                if ((value < 0) || (value > 1))
                    taperLimit = 0;
                else
                    taperLimit = value;
            }
        }
    }
}
