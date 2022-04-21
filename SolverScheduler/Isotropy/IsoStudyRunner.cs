using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainSolver;
using MainSolver.Solvers;

namespace SolverScheduler
{
    class IsoStudyRunner
    {
        public void RunStudy()
        {
            int sampleCount = 1800;
            var creatorSettings = new CreatorSettings();
            creatorSettings.Length = 1;
            creatorSettings.Nodes = 12;
            creatorSettings.DisplacementDistro = Distro.Uniform;
            creatorSettings.DisplacementLimit = 1;
            creatorSettings.dzLimit = 0;
            creatorSettings.WidthDistro = Distro.Uniform;
            creatorSettings.WidthDevLimit = 0; //NO WIDTH DISTRO

            var netFactory = new NetworkListMaker();

            var netList = netFactory.NetworkList(creatorSettings, sampleCount);

            var resultFactory = new IsoResultCreator();
            var Data = resultFactory.EvaluateAngleRange(netList, 1.414, 1);

            var csvWritter = new CsvCreator();

            csvWritter.CreateCsv(Data, "pertRootTwo");

            var meanFlow = Data.Select(x => x.FlowRatioMean);
            var meanAngle = Data.Select(x => x.FlowAngleDeltaMean);
            int h = 7;
        }
    }
}
