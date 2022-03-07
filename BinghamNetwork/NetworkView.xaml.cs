using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MainSolver;

namespace NetworkDisplay
{
    /// <summary>
    /// Interaction logic for NetworkView.xaml
    /// </summary>
    public partial class NetworkView : UserControl
    {
        private Color forwardFlowColor = Colors.Blue;
        private Color backwardFlowColor = Colors.Red;
        private SolidColorBrush strokeBrush = new SolidColorBrush(Colors.Black);
        private SolidColorBrush toolTipBrush = new SolidColorBrush(Colors.White);
        private double strokeThickness = 0.5;
        private double scl;

        public NetworkView()
        {
            InitializeComponent();
            mycanvas.LayoutTransform = new ScaleTransform(1,-1,0,ActualHeight/2);
        }

        public void DrawNetwork(Network network)
        {
            var N = network.Nodes;
            var RadiusSize = 25;
            var margin = 20;
            var max = mycanvas.ActualHeight - (2*RadiusSize+margin)/(double)N;
            scl = max / network.Length;
            scl = scl * N / (N + 1);

            double Radius = RadiusSize / N;
            double scaleMargin = (RadiusSize) / N;

            var translate = scaleMargin + max / (2 * (N - 1));

            
            var maxhFlow = network.hFlow.Max(x => x.Max(Math.Abs));
            var maxvFlow = network.vFlow.Max(x => x.Max(Math.Abs));
            var invMaxFlow = 1/Math.Max(maxvFlow, maxhFlow);
            
            var capacity = N * N + 2 * N * (N - 1);

            mycanvas.Children.Clear();
            mycanvas.Children.Add(BorderPolygon());
            mycanvas.Children.Capacity = capacity+1;

            //Horizontal Lines
            for (int i = 0; i < network.Nodes - 1; i++)
            {
                for (int j = 0; j < network.Nodes; j++)
                {
                    var x1 = network.x[i][j] * scl + translate;
                    var x2 = network.x[i + 1][j] * scl + translate;
                    
                    var y1 = network.y[i][j] * scl + translate;
                    var y2 = network.y[i + 1][j] * scl + translate;

                    var r1 = Radius * (1 - network.hTaper[i][j]) / 2;
                    var r2 = Radius * (1 + network.hTaper[i][j]) / 2;
                    if (r1 + strokeThickness / 2 > Radius)
                        r1 -= strokeThickness / 2;
                    if (r2 + strokeThickness / 2 > Radius)
                        r2 -= strokeThickness / 2;

                    var polyg = ChannelPolygon(x1, y1, r1, x2, y2, r2);

                    if (network.hFlow[i][j] < 0)
                    {
                        polyg.Fill = new SolidColorBrush(backwardFlowColor);
                        polyg.StrokeDashArray = new DoubleCollection() { 2 };
                    }

                    if (network.hFlow[i][j] == 0)
                    {
                        polyg.StrokeDashArray = new DoubleCollection() { 1 };
                    }


                    polyg.ToolTip = HorizontalToolTip(network,i,j);
                    polyg.Fill.Opacity = Math.Abs(network.hFlow[i][j] * invMaxFlow);
                    mycanvas.Children.Add(polyg);
                }
            }

            //Vertical Lines
            for (int i = 0; i < network.Nodes; i++)
            {
                for (int j = 0; j < network.Nodes - 1; j++)
                {
                    var x1 = network.x[i][j] * scl +translate;
                    var x2 = network.x[i][j + 1] * scl + translate;
                    
                    var y1 = network.y[i][j] * scl + translate;
                    var y2 = network.y[i][j + 1] * scl + translate;
                    
                    var r1 = Radius * (1 - network.vTaper[i][j]) / 2;
                    var r2 = Radius * (1 + network.vTaper[i][j]) / 2;

                    if (r1 + strokeThickness / 2 > Radius)
                        r1 -= strokeThickness / 2;
                    if (r2 + strokeThickness / 2 > Radius)
                        r2 -= strokeThickness / 2;


                    var polyg = ChannelPolygon(x1, y1, r1, x2, y2, r2);

                    if (network.vFlow[i][j] < 0)
                    {
                        polyg.Fill = new SolidColorBrush(backwardFlowColor);
                        polyg.StrokeDashArray = new DoubleCollection(){2};
                    }

                    if (network.vFlow[i][j] == 0)
                    {
                        polyg.StrokeDashArray = new DoubleCollection() { 1 };
                    }

                    polyg.Fill.Opacity = Math.Abs(network.vFlow[i][j] * invMaxFlow);
                    polyg.ToolTip = VerticalToolTip(network,i,j);
                    mycanvas.Children.Add(polyg);
                }
            }

            for (int i = 0; i < network.Nodes; i++)
            {
                for (int j = 0; j < network.Nodes; j++)
                {
                    var ellipse = new Ellipse();
                    ellipse.Width = Radius*2;
                    ellipse.Height = Radius*2;
                    ellipse.Fill = new SolidColorBrush(Colors.Black);
                    ellipse.ToolTip = NodeToolTip(network, i, j);

                    var setX = network.x[i][j] * scl - Radius + translate;
                    var setY = (network.y[i][j]) * scl - Radius + translate;

                    Canvas.SetLeft(ellipse, setX);
                    Canvas.SetTop(ellipse, setY);
                    mycanvas.Children.Add(ellipse);


                    if (i == 0 || i==N-1)
                    {
                        if (j == 0 || j == N-1)
                        {
                            var textBlock = new TextBlock();
                            
                            textBlock.Text = Math.Round(network.pressure[i][j],2).ToString();

                            if (i == N - 1 && j == N - 1)
                                textBlock.Text = "   0";

                            textBlock.FontSize = ellipse.Width/2.2;
                            textBlock.Foreground = new SolidColorBrush(Colors.AliceBlue);

                            Canvas.SetLeft(textBlock, setX);
                            Canvas.SetTop(textBlock, setY + ellipse.Height/1.2d);
                            mycanvas.Children.Add(textBlock);
                            textBlock.RenderTransform = new ScaleTransform(1,-1);
                        }
                    }


                }
            }

            int k = 9;
        }

        string NodeToolTip(Network net, int i, int j)
        {
            return "Pressure: " + Math.Round(net.pressure[i][j], 4);
        }

        string HorizontalToolTip(Network net, int i, int j)
        {
            return "Flow: " + Math.Round(net.hFlow[i][j], 4);
        }

        string VerticalToolTip(Network net, int i, int j)
        {
            return "Flow: " + Math.Round(net.vFlow[i][j], 4);
        }

        Polygon ChannelPolygon(double x1, double y1, double r1, double x2, double y2, double r2)
        {
            var polyg = new Polygon();
            var Grad = (y2 - y1) / (x2 - x1);
            var Norm = -1 / Grad;
            var angle = Math.Atan(Norm);
            var p11 = new Point((x1 + r1 * Math.Cos(angle)), (y1 + r1 * Math.Sin(angle)));
            var p12 = new Point((x1 - r1 * Math.Cos(angle)), (y1 - r1 * Math.Sin(angle)));
            var p21 = new Point((x2 + r2 * Math.Cos(angle)), (y2 + r2 * Math.Sin(angle)));
            var p22 = new Point((x2 - r2 * Math.Cos(angle)), (y2 - r2 * Math.Sin(angle)));
            polyg.Points = new PointCollection() { p11, p12, p22, p21 };
            polyg.Stroke = strokeBrush;
            polyg.StrokeThickness = strokeThickness;
            polyg.Fill = new SolidColorBrush(forwardFlowColor);
            return polyg;
        }

        Polygon BorderPolygon()
        {
            var polygon = new Polygon();
            polygon.Points = new PointCollection()
            {
                new Point(0,0),
                new Point(0, mycanvas.ActualWidth),
                new Point(mycanvas.ActualWidth, mycanvas.ActualWidth),
                new Point(mycanvas.ActualHeight,0)
            };
            polygon.Fill = new SolidColorBrush();
            polygon.Fill.Opacity = 0;
            polygon.Stroke = strokeBrush;
            polygon.StrokeThickness = 0.25;
            return polygon;
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Viewbox.Width = Viewbox.ActualHeight;
        }
    }

}
