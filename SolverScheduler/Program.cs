using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MainSolver;

namespace SolverScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            int sampleCount = 40;
            var creatorSettings = new CreatorSettings();
            creatorSettings.Length = 1;
            creatorSettings.Nodes = 12;
            creatorSettings.DisplacementDistro = Distro.Uniform;
            creatorSettings.DisplacementLimit = 1;
            creatorSettings.TaperDistro = Distro.Uniform;
            creatorSettings.TaperLimit = 0;

            var netFactory = new NetworkListMaker();

            var netList = netFactory.NetworkList(creatorSettings, sampleCount);

            var rad = Math.PI / 180;

            var resultFactory = new ResultCreator();
            var result = resultFactory.Evaluate(netList, 2, 30 * rad);


            int y = 9;
            //Results - Flow Mean and StDev, Angle Error and StDev
            // Params, - Network List, PGrad

            //Results Aggregator, Discrepancy, 
            // Params, - Results

            //Results Printer, csv, 
            // Params, -Results


        }

    }
}
