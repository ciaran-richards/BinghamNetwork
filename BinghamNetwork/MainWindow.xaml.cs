using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        API API;
        public MainWindow()
        {
            InitializeComponent();
            API = new API();
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
                displacePerc = double.Parse(displaceText)/100;
                nods = int.Parse(nodeText);
            }
            //Do Nothing
            catch { }

            if ((displacePerc < 0) || (displacePerc>1))
            {
                displacePerc = 0;
            }

            if ((nods < 0) || (nods > 100))
            {
                nods = 3;
            }

            var settings = new CreatorSettings();
            var networkCreator = new NetworkCreator();
            var sett = new CreatorSettings();
            
            sett.Name = "Hi";
            sett.Nodes = nods;
            sett.DisplacementDistro = Distro.Normal;
            sett.TaperDistro = Distro.Normal;
            sett.Length = 100;
            sett.DisplacementLimit = displacePerc;
            sett.TaperLimit = 1;


        }
    }
}
