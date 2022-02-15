using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainSolver
{
    public class SolverAPI
    {
        public SolverAPI()
        {
            var createSet = new CreatorSettings();
            createSet.Nodes = 9;
            createSet.TaperLimit = 0;
            createSet.DisplacementDistro = Distro.Uniform;
            createSet.DisplacementLimit = 1;
            createSet.Length = 100;

            var net = CreateNetwork(createSet);
            net.GradPressure = 300;
            net.PressAngle = 30*Math.PI/180;
            var solver = new NewtonianSolver();
            net = solver.Solve(net);
        }

        public Network CreateNetwork(CreatorSettings createSett)
        {
            var networkCreator = new NetworkCreator();
            var network = networkCreator.CreateNetwork(createSett);
            return network;
        }
    }
}
