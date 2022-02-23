using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainSolver.Solvers;

namespace MainSolver
{
    public class SolverAPI
    {
        public SolverAPI()
        {
            var createSet = new CreatorSettings();
            createSet.Nodes = 15;
            createSet.TaperLimit = 0;
            createSet.DisplacementDistro = Distro.Uniform;
            createSet.DisplacementLimit = 1;
            createSet.Length = 1;

            var net = CreateNetwork(createSet);
            net.GradPressure = 60;
            net.PressAngle = 88.2*Math.PI/180;
            net.YieldPressure = 10;
            var solver = new UniformBinghamSolver();
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
