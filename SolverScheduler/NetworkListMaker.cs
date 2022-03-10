using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainSolver;

namespace SolverScheduler
{
    class NetworkListMaker
    { 
        public Network[] NetworkList(CreatorSettings creatorSettings, int Count)
        {
            Network[] Networks = new Network[Count];
            var networkFactory = new NetworkCreator();
            for (int i = 0; i < Count; i++)
            {
                Networks[i] = networkFactory.CreateNetwork(creatorSettings);
            }
            return Networks;
        }
    }
}
