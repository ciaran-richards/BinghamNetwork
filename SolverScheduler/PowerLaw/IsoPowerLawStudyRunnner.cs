using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainSolver;
using MainSolver.Solvers;

namespace SolverScheduler.PowerLaw
{
    public class IsoPowerLawStudyRunnner
    {
        public void RunStudy()
        {

            var creatorSettings = new CreatorSettings();

            creatorSettings.Length = 1;
            creatorSettings.DisplacementDistro = Distro.Uniform;
            creatorSettings.DisplacementLimit = 1;
            creatorSettings.dzLimit = 0;
            creatorSettings.WidthDistro = Distro.Uniform;
            creatorSettings.WidthDevLimit = 1;

            var netFactory = new NetworkListMaker();

            creatorSettings.Nodes = 32;
            var nets = netFactory.NetworkList(creatorSettings, 70);

            List<IsoResultStruct> results = new List<IsoResultStruct>();
            var isoRun = new IsoPowResultCreator();

            //results = isoRun.EvaluateAngleRange(nets, 1.2, 1);
            //var csv = new CsvCreator();
            //csv.CreateCsv(results, "FinalIso32N");

            var h45 = isoRun.Evaluate(nets, 95, 45*3.14/180, 1.333);
            var h0 = isoRun.Evaluate(nets, 95, 0, 1.333);

            var AP = (h45.FlowRatioMean - h0.FlowRatioMean) / (2 * (h45.FlowRatioMean + h0.FlowRatioMean));

            int k = 1;

        }
    }
}
