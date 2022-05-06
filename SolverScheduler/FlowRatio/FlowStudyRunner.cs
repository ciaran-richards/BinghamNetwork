using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainSolver;

namespace SolverScheduler.FlowRatio
{
    class FlowStudyRunner
    {
        public void RunStudy()
        {

            double index = 1;
            var creatorSettings = new CreatorSettings();
            
            creatorSettings.Length = 1;
            creatorSettings.DisplacementDistro = Distro.Uniform;
            creatorSettings.DisplacementLimit = 1;
            creatorSettings.dzLimit = 0;
            creatorSettings.WidthDistro = Distro.Uniform;
            creatorSettings.WidthDevLimit = 1;

            var netFactory = new NetworkListMaker();

            creatorSettings.Nodes = 8;
            var N8List = netFactory.NetworkList(creatorSettings, 2000);

            creatorSettings.Nodes = 16;
            var N16List = netFactory.NetworkList(creatorSettings, 500);

            creatorSettings.Nodes = 32;
            var N32List = netFactory.NetworkList(creatorSettings, 70);

            //400,1200,3600

            List<double> PGradList = new List<double>()
            {
                1.0, 1.025, 1.05, 1.075, 1.1, 1.125, 1.15, 1.175, 1.2, 1.225, 1.25, 1.275, 1.3, 1.35, 1.4, 1.45
                //1.0, 1.2, 1.4, 1.6 , 1.8, 2.0, 2.2, 2.4, 2.6, 2.8,
                //3.0, 3.7, 4.4, 5.1 , 5.8, 6.5, 7.2, 7.9, 8.6, 9.3,
                //10 , 13.3, 16.6, 20 ,  30,  40 , 50 ,  60,  70,  80,  90, 100
            };

            var flowCreator = new FlowResultCreator();

            var flowResults = new List<FlowResultStruct>(PGradList.Capacity);


            foreach (var pGrad in PGradList)
            {
                flowResults.Add(flowCreator.EvaluateFlow(N8List, N16List, N32List, pGrad, index));
            }

            var csvWritter = new CsvCreator();

            csvWritter.CreateCsv(flowResults, "SmallBinghamFlow");

        }
    }
}
