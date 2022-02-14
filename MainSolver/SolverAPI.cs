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
            createSet.Nodes = 20;
            createSet.TaperLimit = 0;
            createSet.DisplacementDistro = Distro.Normal;
            createSet.DisplacementLimit = 1;
            createSet.Length = 100;

            var net = CreateNetwork(createSet);
            var solver = new NewtonianSolver();
            net = solver.Solve(net);
        }

        private static void Main()
        {
            var createSet = new CreatorSettings();
            createSet.Nodes = 5;
            createSet.TaperLimit = 0;
            createSet.DisplacementDistro = Distro.Normal;
            createSet.DisplacementLimit = 1;
            createSet.Length = 100;

            var api = new SolverAPI();
            var net = api.CreateNetwork(createSet);
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
