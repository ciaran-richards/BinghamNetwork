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
            var studyRunner = new StudyRunner();
            studyRunner.RunStudy();

            Console.ReadLine();

        }

    }
}
