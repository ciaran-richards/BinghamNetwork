using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using MathNet.Numerics.Random;

namespace MainSolver
{
    public class NetworkCreator
    {
        //Share Random Number Generators as much as possible to prevent identical instances and seeds

        private Random randomiser;
        
        //The normal standard deviations and corresponding cumulative probability. Symmetric about 0. 
        private readonly List<double> stanDev;
        private readonly List<double> cuProb;
        
        //File constants
        private const double MAXSD = 3;
        private const int MAXINDEX = 255;
        private const double MAXCUPROB = 0.99865;


        public NetworkCreator()
        {
            randomiser = new Random();
            stanDev = new List<double>(MAXINDEX);
            cuProb = new List<double>(MAXINDEX);
            using (StreamReader input = File.OpenText(Paths.Distributions))
            using (CsvReader reader = new CsvReader(input, CultureInfo.CurrentCulture))
            {
                var fileText = reader.GetRecords<dynamic>();

                foreach (var current in fileText)
                {
                    stanDev.Add(double.Parse(current.key));
                    cuProb.Add(double.Parse(current.value));
                }
            }
        }
        public Network CreateNetwork(CreatorSettings creatorSett)
        {
            var netSet = new NetworkSettings();
            netSet.Nodes = creatorSett.Nodes;
            netSet.Name = creatorSett.Name;
            netSet.Bingham = creatorSett.Bingham;
            netSet.Length = creatorSett.Length;
            var N = creatorSett.Nodes;


            netSet.dx = new double[N][];
            netSet.dy = new double[N][];
            netSet.dz = new double[N][];
            netSet.hWidth = new double[N - 1][];
            netSet.vWidth = new double[N][];
            for (int i = 0; i <= N - 1; i++)
            {
                netSet.dx[i] = new double[N];
                netSet.dy[i] = new double[N];
                netSet.dz[i] = new double[N];
                netSet.vWidth[i] = new double[N - 1];
                if (i < N - 1)
                    netSet.hWidth[i] = new double[N];
            }

            double dxLimit = creatorSett.Length / (2 * ((double)N - 1));
            double dyLimit = dxLimit;
            double dzLimit = dxLimit;
            var max_dx = dxLimit * creatorSett.DisplacementLimit;
            var max_dy = max_dx;
            var max_dz = dzLimit * creatorSett.dzLimit;

            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    {
                        switch (creatorSett.DisplacementDistro)
                        {
                            case Distro.Normal:
                                netSet.dx[i][j] = RandNormDeviation(max_dx, randomiser);
                                netSet.dy[i][j] = RandNormDeviation(max_dy, randomiser);
                                netSet.dz[i][j] = RandNormDeviation(max_dz, randomiser);
                                break;

                            case Distro.Uniform:
                                netSet.dx[i][j] = RandUnifDeviation(max_dx, randomiser);
                                netSet.dy[i][j] = RandUnifDeviation(max_dy, randomiser);
                                netSet.dz[i][j] = RandUnifDeviation(max_dz, randomiser);
                                break;

                        }

                        switch (creatorSett.WidthDistro)
                        {
                            case Distro.Normal:
                                netSet.vWidth[i][j] = 1 + RandNormDeviation(creatorSett.WidthDevLimit, randomiser);
                                netSet.hWidth[i][j] = 1 + RandNormDeviation(creatorSett.WidthDevLimit, randomiser);
                                break;

                            case Distro.Uniform:
                                netSet.vWidth[i][j] = 1 + RandUnifDeviation(creatorSett.WidthDevLimit, randomiser);
                                netSet.hWidth[i][j] = 1 + RandUnifDeviation(creatorSett.WidthDevLimit, randomiser);
                                break;
                        }

                    }
                }
                //Vertically repeating
                netSet.dx[i][N - 1] = netSet.dx[i][0];
                netSet.dy[i][N - 1] = netSet.dy[i][0];
                netSet.dz[i][N - 1] = netSet.dz[i][0];
                netSet.hWidth[i][N - 1] = netSet.hWidth[i][0];
            }

            //Horizontally repeating
            for (int j = 0; j < N; j++)
            {
                netSet.dx[N - 1][j] = netSet.dx[0][j];
                netSet.dy[N - 1][j] = netSet.dy[0][j];
                netSet.dz[N - 1][j] = netSet.dz[0][j];
                if (j < N - 1) 
                    netSet.vWidth[N - 1][j] = netSet.vWidth[0][j];
            }

            //Send network config to constructor to build network
            var network = new Network(netSet);
            return network;
        }

        private double RandUnifDeviation(double maximum, Random ran)
        {
            var dxRan = ran.NextDouble();
            double sign = ran.NextBoolean() ? 1:-1;
            return dxRan * maximum * sign;
        }

        private double RandNormDeviation(double maximum, Random ran)
        {
            //Generate a random deviation from mean in a Normal (Gaussian) Distribution

            var dxRan = ran.NextDouble();
            //The discretised normal distribution has a range of 0.00003 to 0.99997, random number be between.
            bool isOutRange = ((dxRan<(1-MAXCUPROB))||(dxRan > MAXCUPROB));
            while (isOutRange)
            {
                dxRan = ran.NextDouble();
                isOutRange = ((dxRan < (1 - MAXCUPROB)) || (dxRan > MAXCUPROB));

            }
            var xnegetive = false;
            if (dxRan < 0.5)
            {
                dxRan = 1 - dxRan;
                xnegetive = true;
            }

            //Linear Interpolation to calculate how many Standard Deviations
            var higherValue = cuProb.First(x => x > dxRan);
            var higherIndex = cuProb.IndexOf(higherValue);
            var lowerValue = cuProb.Last(x => x < dxRan);
            var lowerIndex = cuProb.IndexOf(lowerValue);
            var fraction = (dxRan - lowerValue) / (higherValue - lowerValue);
            var deviations = stanDev[lowerIndex] + fraction * (stanDev[higherIndex] - stanDev[lowerIndex]);
            
            var delta = deviations * maximum / MAXSD;
            if (xnegetive) delta = -delta;
            return delta;
        }

    }
}
