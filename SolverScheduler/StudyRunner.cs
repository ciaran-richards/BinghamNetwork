using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainSolver;
using MainSolver.Solvers;

namespace SolverScheduler
{
    class StudyRunner
    {
        public void RunStudy()
        {
            int sampleCount = 3;
            var creatorSettings = new CreatorSettings();
            creatorSettings.Length = 1;
            creatorSettings.Nodes = 70;
            creatorSettings.DisplacementDistro = Distro.Uniform;
            creatorSettings.DisplacementLimit = 1;
            creatorSettings.dzLimit = 0;
            creatorSettings.WidthDistro = Distro.Uniform;
            creatorSettings.WidthDevLimit = 1;


            var netFactory = new NetworkListMaker();

            var netList = netFactory.NetworkList(creatorSettings, sampleCount);

            var resultFactory = new IsoResultCreator();
            var Data = resultFactory.EvaluateAngleRange(netList, 1.1);

            var csvWritter = new CsvCreator();

            csvWritter.CreateCsv(Data, "Network_70_PointOne_1");

            var meanFlow = Data.Select(x => x.FlowRatioMean);
            var meanAngle = Data.Select(x => x.FlowAngleDeltaMean);
            int h = 7;
        }
    }
}
