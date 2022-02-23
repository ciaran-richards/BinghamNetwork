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
            createSet.Nodes = 5;
            createSet.TaperLimit = 0;
            createSet.DisplacementDistro = Distro.Uniform;
            createSet.DisplacementLimit = 0.5;
            createSet.Length = 100;

            var net = CreateNetwork(createSet);
            net.GradPressure = 4;
            net.PressAngle = 88.2*Math.PI/180;
            net.YieldPressure = 0.3;
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
