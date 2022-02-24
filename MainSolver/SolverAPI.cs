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
            createSet.DisplacementLimit = 0;
            createSet.Length = 1;

            var net = CreateNetwork(createSet);
            //Sigma = 2
            net.GradPressure = 20;
            net.PressAngle = 60*Math.PI/180;
            net.YieldPressure =2;
            var binghamSolver = new UniformBinghamSolver();
            var newtonianSolver = new NewtonianSolver();
            var bing = binghamSolver.Solve(net.Copy());
            var newt = newtonianSolver.Solve(net.Copy());

            var ratio = bing.FlowRate / newt.FlowRate;

            int y = 9;
        }

        public Network CreateNetwork(CreatorSettings createSett)
        {
            var networkCreator = new NetworkCreator();
            var network = networkCreator.CreateNetwork(createSett);
            return network;
        }
    }
}
