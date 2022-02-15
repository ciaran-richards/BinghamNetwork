using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using MainSolver;

namespace NetworkDisplay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        { 
            var winApi = new WindowAPI();
            var solverApi = new SolverAPI();
        }

    }
}
