using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainSolver
{
    public class Network
    {
        //Network Properties

        public string Name { get; set; }
        public int Nodes { get; private set; } //
        public double Length { get; private set; } //
        public  double Bingham { get; private set; } //
        public double GradPressure { get; private set; }//
        public double GPresAngle { get; private set; }//
        public double FlowRate { get; set; }
        public double FlowAngle { get; set; }


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
            Bingham = sett.Bingham;
            Length = sett.Length;
            dx = sett.dx;
            dy = sett.dy;
            hTaper = sett.hTaper;
            vTaper = sett.vTaper;

            // Set Dimensions of template and assign to all fields
            x = new double[N][];
            y = new double[N][];
            hLength = new double[N-1][]; 
            vLength = new double[N][];
            inv_hLength = new double[N - 1][];
            inv_vLength = new double[N][];
            pressure = new double[N][]; 
            residual = new double[N][];
            hFlow = new double[N-1][];
            vFlow = new double[N][];
            h_dP = new double[N-1][];
            v_dP = new double[N][];

            for (int i = 0; i <=N-1; i++)
            {
                x[i] = new double[N];
                y[i] = new double[N];
                pressure[i] = new double[N];
                residual[i] = new double[N];
                vLength[i] = new double[N-1];
                inv_vLength[i] = new double[N - 1];
                vFlow[i] = new double[N-1];
                v_dP[i] = new double[N-1];

                if (i<N-1)
                {
                    hLength[i] = new double[N];
                    inv_hLength[i] = new double[N];
                    hFlow[i]= new double[N];
                    h_dP[i] = new double[N];
                }

            }

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

    }
}
