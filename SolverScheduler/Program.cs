using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MainSolver;
using SolverScheduler.FlowRatio;

namespace SolverScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            var studyRunner = new IsoStudyRunner();
            studyRunner.RunStudy();

            var flowStudyRunner = new FlowStudyRunner();
            //flowStudyRunner.RunStudy();

            Console.ReadLine();

        }

    }
}
