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
        public double DisplacementLimit;
        private double widthDevLimit;
        public Distro WidthDistro;
        public double dzLimit;
        public double WidthDevLimit
        {
            get { return widthDevLimit; }
            set
            {
                if ((value < 0) || (value > 1))
                    widthDevLimit = 0;
                else
                    widthDevLimit = value;
            }
        }


    }
}
