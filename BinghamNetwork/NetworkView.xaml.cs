using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MainSolver;

namespace NetworkDisplay
{
    /// <summary>
    /// Interaction logic for NetworkView.xaml
    /// </summary>
    public partial class NetworkView : UserControl
    {
        public NetworkView()
        {
            InitializeComponent();
            GeometryDrawing.Pen = new Pen(Brushes.Black, 1);
        }

        public void DrawNetwork(Network network)
        {
            var N = network.Nodes;
            var max = mycanvas.ActualHeight;
            var scl = max / network.Length;
            double Radius = 25 * scl / N;
            int LineThickness = (int)(Radius / 4);
            var capacity = N * N + 4 * N * (N - 1);
            if (N >= 30)
            {
                capacity = N * N + 2 * N * (N - 1);
                LineThickness = LineThickness * 2;
            }
            GeometryDrawing.Pen.Thickness = Math.Max(LineThickness, 1);
            GeometryGroup.Children = new GeometryCollection(capacity);
            mycanvas.Children.Clear();

            //Horizontal Lines
            for (int i = 0; i < network.Nodes - 1; i++)
            {
                for (int j = 0; j < network.Nodes; j++)
                {
                    var x1 = network.x[i][j] * scl;
                    var x2 = network.x[i + 1][j] * scl;
                    var y1 = network.y[i][j] * scl;
                    var y2 = network.y[i + 1][j] * scl;

                    var Grad = (y2 - y1) / (x2 - x1);
                    var Norm = -1 / Grad;
                    var angle = Math.Atan(Norm);

                    var polyg = new Polygon();

                    var r1 = Radius * (1 - network.hTaper[i][j]) / 1.8;
                    var r2 = Radius * (1 + network.hTaper[i][j]) / 1.8;

                    var p11 = new Point((x1 + r1 * Math.Cos(angle)), (y1 + r1 * Math.Sin(angle)));
                    var p12 = new Point((x1 - r1 * Math.Cos(angle)), (y1 - r1 * Math.Sin(angle)));
                    var p21 = new Point((x2 + r2 * Math.Cos(angle)), (y2 + r2 * Math.Sin(angle)));
                    var p22 = new Point((x2 - r2 * Math.Cos(angle)), (y2 - r2 * Math.Sin(angle)));
                    polyg.Points = new PointCollection() { p11, p12, p22, p21 };
                    polyg.Fill = new SolidColorBrush(Colors.CornflowerBlue);
                    polyg.ToolTip = network.hFlow[i][j];
                    mycanvas.Children.Add(polyg);
                }
            }

            //Vertical Lines
            for (int i = 0; i < network.Nodes; i++)
            {
                for (int j = 0; j < network.Nodes - 1; j++)
                {
                    var x1 = network.x[i][j] * scl;
                    var x2 = network.x[i][j + 1] * scl;
                    var y1 = network.y[i][j] * scl;
                    var y2 = network.y[i][j + 1] * scl;

                    var Grad = (y2 - y1) / (x2 - x1);
                    var Norm = -1 / Grad;
                    var angle = Math.Atan(Norm);
                    var polyg = new Polygon();

                    var r1 = Radius * (1 - network.vTaper[i][j]) / 1.8;
                    var r2 = Radius * (1 + network.vTaper[i][j]) / 1.8;

                    var p11 = new Point((x1 + r1 * Math.Cos(angle)), (y1 + r1 * Math.Sin(angle)));
                    var p12 = new Point((x1 - r1 * Math.Cos(angle)), (y1 - r1 * Math.Sin(angle)));
                    var p21 = new Point((x2 + r2 * Math.Cos(angle)), (y2 + r2 * Math.Sin(angle)));
                    var p22 = new Point((x2 - r2 * Math.Cos(angle)), (y2 - r2 * Math.Sin(angle)));

                    polyg.Points = new PointCollection() { p11, p12, p22, p21 };
                    polyg.Fill = new SolidColorBrush(Colors.CornflowerBlue);
                    polyg.ToolTip = network.vFlow[i][j];

                    mycanvas.Children.Add(polyg);
                }
            }

            for (int i = 0; i < network.Nodes; i++)
            {
                for (int j = 0; j < network.Nodes; j++)
                {
                    var ellipse = new Ellipse();
                    ellipse.Width = Radius * 2;
                    ellipse.Height = Radius * 2;
                    ellipse.Fill = new SolidColorBrush(Colors.Black);

                    Canvas.SetLeft(ellipse, network.x[i][j] * scl - Radius);
                    Canvas.SetTop(ellipse, (network.y[i][j]) * scl - Radius);
                    mycanvas.Children.Add(ellipse);
                }
            }

            int k = 9;
        }

    }

}
