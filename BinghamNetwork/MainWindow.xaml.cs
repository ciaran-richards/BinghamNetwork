using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MainSolver;
using MainSolver.Solvers;

namespace NetworkDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SolverAPI solverApi;
        private Network selectedNetwork;
        private bool isNewtonian;
        public MainWindow()
        {
            InitializeComponent();
            solverApi = new SolverAPI();
            UpdateCompareButton();
        }

        private void NetworkView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => NewNetworkRequest();

        private void NewNetworkRequest()
        {
            var displaceText = DisplaceText.Text;
            double displacePerc = 0;

            var nodeText = NodesText.Text;
            int nods = 3;
            double shearIndex = 1;
            double pGrad = 1;
            double pangle = 1;
            try
            {
                displacePerc = double.Parse(displaceText);
                nods = int.Parse(nodeText);
                shearIndex = double.Parse(SIndex.Text);
                pGrad = double.Parse(PGrad.Text);
                pangle = double.Parse(PAngle.Text) * Math.PI / 180;
            }
            //Do Nothing
            catch { }

            if ((nods < 0) || (nods > 100))
            {
                nods = 3;
            }



            var settings = new CreatorSettings();
            var networkCreator = new NetworkCreator();
            var sett = new CreatorSettings();

            sett.Name = "Hi";
            sett.Nodes = nods;
            sett.DisplacementDistro = IsUniform.IsChecked.Value ? Distro.Uniform : Distro.Normal;
            sett.WidthDistro = IsUniform.IsChecked.Value ? Distro.Uniform : Distro.Normal;
            sett.Length = 1;
            sett.DisplacementLimit = displacePerc;
            sett.dzLimit = displacePerc;
            sett.WidthDevLimit = displacePerc;

            var net = solverApi.CreateNetwork(sett);

            net.GradPressure = pGrad;
            net.PressAngle = pangle;

            net.ShearIndex = shearIndex;
            net.YieldPressure = 0.5;
            
            selectedNetwork = net;
            UpdateCanvas();
        }

        private void UpdateCompareButton()
        {
            CompareButton.Content = isNewtonian ? "Newtonian/Indexed" : "Bingham/PseudoPlastic";
            var colour = isNewtonian ? Colors.LightBlue : Colors.LightGreen;
            CompareButton.Background = new SolidColorBrush(colour);
        }

        private void ChangeDisplay_OnClick(object sender, RoutedEventArgs e)
        {
            isNewtonian = !isNewtonian;
            UpdateCompareButton();
            UpdateCanvas();
        }

        private void UpdateCanvas()
        {
            var net = selectedNetwork.Copy();
            if (!HasDepth.IsChecked.Value)
                net = net.CopyNoDepth();
            if (!HasWidth.IsChecked.Value)
                net = net.CopyNoWidth();
            net.GradPressure = net.GradPressure;

            if (isNewtonian)
            {
                if (Math.Abs(net.ShearIndex - 1) < 0.0001)
                {
                    var solver = new NewtonianSolver();
                    net = solver.Solve(net);
                }
                else
                {
                    var solver = new ShearIndexSolver();
                    net.YieldPressure = 0d;
                    solver.MaxRedidual = Math.Pow(10, -6);
                    net = solver.Solve(net);
                }
            }
            else
            {
                var solver = new HerschelBulkleySolver();
                solver.MaxRedidual = Math.Pow(10, -5);
                solver.reg = Math.Pow(10, -7);
                //var postProcessor = new PostProcessor();
                net = solver.Solve(net);

                //net = postProcessor.PostProcess(net);
            }

            if(net != null)
            {
                NetworkRegion.DrawNetwork(net);
                Name.Text = "Name: " + net.Name;
                Bingham.Text = "Yield: " + net.YieldPressure;
                Nodes.Text = "Nodes: " + net.Nodes + " ^2: " + (net.Nodes * net.Nodes);
                FlowRate.Text = "FlowRate: " + net.FlowRate;
                FlowAngle.Text = "FlowAngle: " + net.FlowAngle * 180 / Math.PI + " deg";
                PressureGrad.Text = "Pressure Gradient: " + net.GradPressure;
                PressureAngle.Text = "Pressure Angle: " + net.PressAngle * 180 / Math.PI + " deg";
                MaxRes.Text = "Maximum Residual: " + net.MaxResidual;
            }
            else
            {
                NetworkRegion.DrawNetwork(selectedNetwork);
                Name.Text = "Complete";
            }
        }


        private void NetworkRegion_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            selectedNetwork.GradPressure += 0.025 *(double)e.Delta/120d;
            PGrad.Text = Math.Round(selectedNetwork.GradPressure,5).ToString();
            UpdateCanvas();
        }


        private void HasWidth_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateCanvas();
        }

        private void HasDepth_OnClick(object sender, RoutedEventArgs e)
        {
           UpdateCanvas();
        }

        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Up)
            {
                selectedNetwork.PressAngle  += 5 * Math.PI / 180;
                PAngle.Text = Math.Round(selectedNetwork.PressAngle * 180 / Math.PI, 5).ToString();
                UpdateCanvas();
            }

            if (e.Key == Key.Down)
            {
                selectedNetwork.PressAngle -= 5 * Math.PI / 180;
                PAngle.Text = Math.Round(selectedNetwork.PressAngle * 180 / Math.PI, 5).ToString();
                UpdateCanvas();
            }

            if (e.Key == Key.OemCloseBrackets)
            {
                selectedNetwork.ShearIndex += 0.1;
                SIndex.Text = Math.Round(selectedNetwork.ShearIndex, 5).ToString();
                UpdateCanvas();
            }

            if (e.Key == Key.OemOpenBrackets)
            {
                selectedNetwork.ShearIndex -= 0.1;
                SIndex.Text = Math.Round(selectedNetwork.ShearIndex, 5).ToString();
                UpdateCanvas();
            }


        }
    }
}
