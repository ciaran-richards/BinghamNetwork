using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainSolver;
using MainSolver.Solvers;

namespace SolverScheduler.PowerLaw
{
    public class PowerLawStudyRunnner
    {
        public void RunStudy()
        {

            double index = 0.95;
            var creatorSettings = new CreatorSettings();

            creatorSettings.Length = 1;
            creatorSettings.DisplacementDistro = Distro.Uniform;
            creatorSettings.DisplacementLimit = 1;
            creatorSettings.dzLimit = 0;
            creatorSettings.WidthDistro = Distro.Uniform;
            creatorSettings.WidthDevLimit = 1;

            var netFactory = new NetworkListMaker();

            creatorSettings.Nodes = 8;
            var N8List = netFactory.NetworkList(creatorSettings, 5000);

            creatorSettings.Nodes = 16;
            var N16List = netFactory.NetworkList(creatorSettings, 200);


            List<double> PGradList = new List<double>()
            {
                0.1, 0.2, 0.4, 0.6, 0.8, 1, 1.2, 1.4, 1.6, 1.8, 2, 4, 6, 8, 10, 13, 16, 20, 35, 50, 65, 80, 95 
            };

            var powerResultCreator = new PowerLawResultCreator();
            var ind = 0.95;

            List<FlowResultStruct> results = new List<FlowResultStruct>();

            foreach (var pgrad in PGradList)
            {
                var f0 = powerResultCreator.EvaluateAngle(N16List, pgrad, 0, ind);
                var f45 = powerResultCreator.EvaluateAngle(N16List, pgrad, 45, ind);
                var av = 0.5 * (f0.FlowRatioMean + f45.FlowRatioMean);
                var gg = new FlowResultStruct();
                gg.ShearIndex = ind;
                gg.FlowRatio = av;
                gg.BinghamGrad = pgrad;
                results.Add(gg);
            }

            var csv = new CsvCreator();
            csv.CreateCsv(results, "v3Third");

            int k = 1;

        }
    }
}
