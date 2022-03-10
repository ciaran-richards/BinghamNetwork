using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainSolver
{
    public static class Paths
    {
        public static string Application = Environment.CurrentDirectory;
        public static string InternalFiles = Application + "/Files";
        public static string Distributions = InternalFiles + "/Distributions.csv";

        public static string Results = "C:\\ME FYP\\Results";
    }
}
