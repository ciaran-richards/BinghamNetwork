using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainSolver
{
    public struct NetworkSettings
    {
        public string Name;
        public int Nodes; //
        public double Length; //
        public double Bingham; //
        public double GradP;
        public double GradPAngle;

        //Node Properties
        public double[][] dx; //
        public double[][] dy; //
        public double[][] dz;

        //Channel Properties
        public double[][] hWidth; //
        public double[][] vWidth;
    }
}
