using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainSolver
{
    public class Network
    {
        //Network Properties

        private double yieldPressure = 1;
        private double inv_yieldPressure = 1;
        private double gradPressure;
        private double pressAngle;

        public string Name { get; set; }
        public int Nodes { get; private set; } //
        public double Length { get; private set; } //

        public double YieldPressure
        {
            get { return yieldPressure;}
            set
            {
                yieldPressure = value;
                inv_yieldPressure = 1 / value;
                ResetVariables();
            }
        }

        public double Inv_Yield { get {return inv_yieldPressure;} private set {} }

        public double GradPressure
        {
            get { return gradPressure; }
            set
            {
                gradPressure = value;
                ResetVariables();
            }
        }//


        public double PressAngle
        {
            get { return pressAngle; }
            set
            {
                pressAngle = value;
                ResetVariables();
            }
        }//
        public double FlowRate { get; private set; }
        public double FlowAngle { get; private set; }
        public double MaxResidual { get; private set; }
        public double AveResidual { get; private set; }

        //Node Properties
        public double[][] x { get; private set; }
        public double[][] y { get; private set; }
        
        private double[][] dx; //
        private double[][] dy; //

        //Channel Properties
        public double[][] hTaper { get; private set; } //
        public double[][] vTaper { get; private set; } //
        public double[][] hLength { get; private set; }
        public double[][] inv_hLength { get; private set; } //Pre-compute as this will be used often
        public double[][] vLength { get; private set; }
        public double[][] inv_vLength { get; private set; }

        //Node Quantities
        public double[][] pressure { get; set; }
        public double[][] residual { get; set; }

        // Channel Quantities
        public double[][] hFlow { get; set; }
        public double[][] vFlow { get; set; }
        public double[][] h_dP { get; set; }
        public double[][] v_dP { get; set; }

        public Network(NetworkSettings sett)
        {
            Name = sett.Name;
            Nodes = sett.Nodes;
            var N = Nodes;
            Length = sett.Length;
            dx = sett.dx;
            dy = sett.dy;
            hTaper = sett.hTaper;
            vTaper = sett.vTaper;

            //Initialise all fields and set to zero;
            InitialiseFields();

            yieldPressure = sett.Bingham;
            inv_yieldPressure = 1 / sett.Bingham;

            double invn1 = 1 / ((double)N - 1);
            
            //Set Node positions
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    x[i][j] = i * Length * invn1 + dx[i][j];
                    y[i][j] = j * Length * invn1 + dy[i][j];
                }
            }

            //Calculate horizonal lengths
            for (int i = 0; i < N-1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    var deltaX = x[i + 1][j] - x[i][j];
                    var deltaY = y[i + 1][j] - y[i][j];
                    hLength[i][j] = Math.Sqrt(deltaX*deltaX + deltaY*deltaY);
                    inv_hLength[i][j] = 1 / hLength[i][j];
                }
            }

            //Calculate vertical lengths
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    var deltaX = x[i][j+1] - x[i][j];
                    var deltaY = y[i][j+1] - y[i][j];
                    vLength[i][j] = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                    inv_vLength[i][j] = 1 / vLength[i][j];
                }
            }

        }

        private void InitialiseFields()
        {
            var N = Nodes;
            x = new double[N][];
            y = new double[N][];
            hLength = new double[N - 1][];
            vLength = new double[N][];
            inv_hLength = new double[N - 1][];
            inv_vLength = new double[N][];
            pressure = new double[N][];
            residual = new double[N][];
            hFlow = new double[N - 1][];
            vFlow = new double[N][];
            h_dP = new double[N - 1][];
            v_dP = new double[N][];

            for (int i = 0; i <= N - 1; i++)
            {
                x[i] = new double[N];
                y[i] = new double[N];
                vLength[i] = new double[N - 1];
                inv_vLength[i] = new double[N - 1];
                if (i < N - 1)
                {
                    hLength[i] = new double[N];
                    inv_hLength[i] = new double[N];
                }
            }

            ResetVariables();
        }

        void ResetVariables()
        {
            AveResidual = 0;
            MaxResidual = 0;
            FlowRate = 0;
            FlowAngle = 0;
            var N = Nodes;
            for (int i = 0; i <= N - 1; i++)
            {
                pressure[i] = new double[N];
                residual[i] = new double[N];
                vFlow[i] = new double[N - 1];
                v_dP[i] = new double[N - 1];

                if (i < N - 1)
                {
                    hFlow[i] = new double[N];
                    h_dP[i] = new double[N];
                }
            }
        }

        public void CalculateResiduals()
        {
            int N = Nodes;
            for (int i = 1; i < N - 1; i++)
            {
                for (int j = 1; j < N - 1; j++)
                {
                    residual[i][j] = hFlow[i - 1][j] - hFlow[i][j] + vFlow[i][j - 1] - vFlow[i][j];
                }
            }

            for (int i = 1; i < N - 1; i++)
            {
                residual[i][0] = hFlow[i - 1][0] - hFlow[i][0] + vFlow[i][N - 2] - vFlow[i][0];
                residual[i][N - 1] = hFlow[i - 1][N - 1] - hFlow[i][N - 1] + vFlow[i][N - 2] - vFlow[i][0];
            }

            for (int j = 1; j < N - 1; j++)
            {
                residual[0][j] = hFlow[N - 2][j] - hFlow[0][j] + vFlow[0][j - 1] - vFlow[0][j];
                residual[N - 1][j] = hFlow[N - 2][j] - hFlow[0][j] + vFlow[N - 1][j - 1] - vFlow[N - 1][j];
            }

            residual[0][0] = hFlow[N - 2][0] - hFlow[0][0] + vFlow[0][N - 2] - vFlow[0][0]; //BL
            residual[N - 1][0] = hFlow[N - 2][0] - hFlow[0][0] + vFlow[N - 1][N - 2] - vFlow[N - 1][0]; //BR
            residual[0][N - 1] = hFlow[N - 2][N - 1] - hFlow[0][N - 1] + vFlow[0][N - 2] - vFlow[0][0]; //TL
            residual[N - 1][N - 1] = hFlow[N - 2][N - 1] - hFlow[0][N - 1] + vFlow[N - 1][N - 2] - vFlow[N - 1][0]; //BR

            AveResidual = residual.Average(x => x.Average());
            MaxResidual = residual.Max(x => x.Max());

        }

        public void CalculateBulkFlow()
        {
            var N = Nodes;
            double VertFlow = 0;
            double HorizFlow = 0;
            double angleRad;
            double dy;
            double dx;

            // Horizontal Channels
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    var deltaX = x[i + 1][j] - x[i][j];
                    var deltaY = y[i + 1][j] - y[i][j];
                    angleRad = Math.Atan2(deltaY, deltaX);
                    HorizFlow += hFlow[i][j] * Math.Cos(angleRad);
                    VertFlow += hFlow[i][j] * Math.Sin(angleRad);
                }
            }

            //Vertical Channels
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    var deltaX = x[i][j + 1] - x[i][j];
                    var deltaY = y[i][j + 1] - y[i][j];
                    angleRad = Math.Atan2(deltaY, deltaX);
                    HorizFlow += vFlow[i][j] * Math.Cos(angleRad);
                    VertFlow += vFlow[i][j] * Math.Sin(angleRad);
                }
            }
            FlowRate = Math.Sqrt(HorizFlow * HorizFlow + VertFlow * VertFlow);
            FlowAngle = Math.Atan(VertFlow/HorizFlow);
        }

    }
}
