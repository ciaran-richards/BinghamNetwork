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
            int sampleCount = 90;
            var creatorSettings = new CreatorSettings();
            creatorSettings.Length = 1;
            creatorSettings.Nodes = 32;
            creatorSettings.DisplacementDistro = Distro.Uniform;
            creatorSettings.DisplacementLimit = 1;
            creatorSettings.dzLimit = 0;
            creatorSettings.WidthDistro = Distro.Normal; //Normal is Set !!
            creatorSettings.WidthDevLimit = 1; //NO WIDTH DISTRO

            var netFactory = new NetworkListMaker();

            var netList = netFactory.NetworkList(creatorSettings, sampleCount);

            var resultFactory = new IsoResultCreator();
            var Data = resultFactory.EvaluateAngleRange(netList, 1.2, 1);

            var csvWritter = new CsvCreator();

            csvWritter.CreateCsv(Data, "WidthVaryNormal");

            //sampleCount = 8000;
            //creatorSettings = new CreatorSettings();
            //creatorSettings.Length = 1;
            //creatorSettings.Nodes = 8;
            //creatorSettings.DisplacementDistro = Distro.Uniform;
            //creatorSettings.DisplacementLimit = 1;
            //creatorSettings.dzLimit = 0;
            //creatorSettings.WidthDistro = Distro.Uniform;
            //creatorSettings.WidthDevLimit = 1; //NO WIDTH DISTRO


            //var netList = netFactory.NetworkList(creatorSettings, sampleCount);
            //var resultFactory = new IsoResultCreator();
            //var Data = new List<IsoResultStruct>();
            //Data = resultFactory.EvaluateAngleRange(netList, 2, 0.2);

            //var csvWritter = new CsvCreator();

            //csvWritter.CreateCsv(Data, "ConvInd8_Run2");

        }
    }
}
