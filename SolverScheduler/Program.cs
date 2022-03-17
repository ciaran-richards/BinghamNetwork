
using TaperedLookup;

namespace SolverScheduler
{
    class Program
    {
        static void Main(string[] args)
        {

            var iterator = new ElementIteration();

            var diction = new ChannelSolver();
            var result = diction.PressureDrop(1, 0.5, 1);

            var runner = new StudyRunner();
            //runner.RunStudy();
        }

    }
}
