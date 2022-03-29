﻿using System;
using System.Windows;
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var displaceText = DisplaceText.Text;
            double displacePerc = 0;

            var nodeText = NodesText.Text;
            int nods = 3;
            try
            {
                displacePerc = double.Parse(displaceText);
                nods = int.Parse(nodeText);
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
            sett.DisplacementDistro = CheckBox.IsChecked.Value ? Distro.Uniform : Distro.Normal;
            sett.WidthDistro = CheckBox.IsChecked.Value ? Distro.Uniform : Distro.Normal; 
            sett.Length = 1;
            sett.DisplacementLimit = displacePerc;
            sett.dzLimit = 0;
            sett.WidthDevLimit = 1;
            
            var net = solverApi.CreateNetwork(sett);
            net.YieldPressure = 0.5;
            try
            {
                net.GradPressure = double.Parse(PGrad.Text);
                net.PressAngle = double.Parse(PAngle.Text)*Math.PI/180;
            }
            catch(Exception exception)
            {}
            selectedNetwork = net;
           UpdateCanvas();
        }

        private void UpdateCompareButton()
        {
            CompareButton.Content = isNewtonian ? "Newtonian" : "Bingham";
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
            if (isNewtonian)
            {
                var solver = new NewtonianSolver();
                var net = selectedNetwork.Copy();
                net.GradPressure = net.GradPressure;
                net = solver.Solve(net);
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
                var solver = new UniformBinghamSolver();
                solver.MaxRedidual = Math.Pow(10, -6);
                solver.reg = Math.Pow(10, -9);
                var postProcessor = new PostProcessor();
                var net = solver.Solve(selectedNetwork);
                net = postProcessor.PostProcess(net);
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

        }


        private void NetworkRegion_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            selectedNetwork.GradPressure += 0.025 *(double)e.Delta/120d;
            PGrad.Text = Math.Round(selectedNetwork.GradPressure,5).ToString();
            UpdateCanvas();
        }

    

        private void NetworkRegion_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W || e.Key == Key.Up)
            {
                selectedNetwork.PressAngle += 2 * Math.PI / 180;
            }

            if (e.Key == Key.S || e.Key == Key.Down)
            {
                selectedNetwork.PressAngle -= 2 * Math.PI / 180;
            }

            PressureAngle.Text = Math.Round(selectedNetwork.PressAngle, 5).ToString();
            UpdateCanvas();
        }
    }
}
