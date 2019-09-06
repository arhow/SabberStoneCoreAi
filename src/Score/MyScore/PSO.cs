using System;
using System.IO;
using System.Text;
using SabberStoneCoreAi.Agent.MyAgents;

namespace SabberStoneCoreAi.Score.MyScore
{
    public class Particle
    {
        public double[] Position;
        public double[] Velocity;
        public double[] BestPosition;
        public double Error;
        public double BestError;

        public Particle(double[] position, double error, double[] velocity, double[] bestPosition, double bestError)
        {
            Position = new double[position.Length];
            position.CopyTo(Position, 0);

            Error = error;
            Velocity = new double[velocity.Length];
            velocity.CopyTo(Velocity, 0);

            BestPosition = new double[bestPosition.Length];
            bestPosition.CopyTo(BestPosition, 0);
            BestError = bestError;
        }

        public static Particle GenParticle(string line, int dimension)
        {
            string[] cells = line.Split(',');
            int cellIndex = 0;
            double[] position = new double[dimension];
            double[] velocity = new double[dimension];
            double[] bestPosition = new double[dimension];
            for(int i = 0; i < dimension; i++)
            {
                position[i] = Double.Parse(cells[cellIndex++]);
            }
            for(int i = 0; i < dimension; i++)
            {
                velocity[i] = Double.Parse(cells[cellIndex++]);
            }
            for(int i = 0; i < dimension; i++)
            {
                bestPosition[i] = Double.Parse(cells[cellIndex++]);
            }
            double error = Double.Parse(cells[cellIndex++]);

            double bestError = Double.Parse(cells[cellIndex++]);

            return new Particle(position, error, velocity, bestPosition, bestError);
        }


        public string FullPrint()
        {
            StringBuilder sb = new StringBuilder();

            foreach (double p in Position)
            {
                sb.Append(p.ToString("G17"));
                sb.Append(",");
            }

            foreach (double v in Velocity)
            {
                sb.Append(v.ToString("G17"));
                sb.Append(",");
            }

            foreach (double bp in BestPosition)
            {
                sb.Append(bp.ToString("G17"));
                sb.Append(",");
            }

            sb.Append(Error.ToString("G17"));
                sb.Append(",");

            sb.Append(BestError.ToString("G17"));

            return sb.ToString();
        }
    }

    class PSO
    {

        public static void Restore(out Particle[] currentSwarm, out int currentEpoch, out double currentMinGlobalError, out double[] bestGlobalPosition, string path="pso.sav")
        {
            // currentEpoch
            // currentMinGlobalError
            // dimensions
            // particleCount
            // currentSwarm
            // bestGlobalPosition
            // minX
            // maxX

            string[] lines = Log.ReadAllLines(path);
            int lineIndex = 0;

            currentEpoch = Int32.Parse(lines[lineIndex++]);
            currentMinGlobalError = Double.Parse(lines[lineIndex++]);
            int dimensions = Int32.Parse(lines[lineIndex++]);
            int particleCount = Int32.Parse(lines[lineIndex++]);

            // currentSwarm
            currentSwarm = new Particle[particleCount];
            for(int i = 0; i < particleCount; i++)
            {
                currentSwarm[i] = Particle.GenParticle(lines[lineIndex++], dimensions);
            }

            // bestGlobalPosition
            string[] strBestGlobalPosition = lines[lineIndex++].Split(",");
            bestGlobalPosition = new double[dimensions];
            for(int i = 0; i < dimensions; i++)
            {
                bestGlobalPosition[i] = Double.Parse(strBestGlobalPosition[i]);
            }

            // minX
            double[] minX = new double[dimensions];
            string[] strMinX = lines[lineIndex++].Split(",");
            for(int i = 0; i < dimensions; i++)
            {
                minX[i] = Double.Parse(strMinX[i]);
            }

            // maxX
            double[] maxX = new double[dimensions];
            string[] strMaxX = lines[lineIndex++].Split(",");
            for(int i = 0; i < dimensions; i++)
            {
                maxX[i] = Double.Parse(strMaxX[i]);
            }

            return;
        }

        public static void Save(Particle[] currentSwarm, int particleCount, int dimensions, double[] minX, double[] maxX, int currentEpoch, double currentMinGlobalError, double[] currentBestGlobalPosition, string path="pso.sav")
        {
            // currentEpoch
            // currentMinGlobalError
            // dimensions
            // particleCount
            // currentSwarm
            // bestGlobalPosition
            // minX
            // maxX
            Log.Instance().New(path);

            Log.Instance().Append(currentEpoch.ToString("G17"));
            Log.Instance().Append(currentMinGlobalError.ToString("G17"));
            Log.Instance().Append(dimensions.ToString("G17"));
            Log.Instance().Append(particleCount.ToString("G17"));

            // currentSwarm
            foreach (Particle p in currentSwarm)
                Log.Instance().Append(p.FullPrint());

            // bestGlobalPosition
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dimensions-1; ++i)
            {
                sb.Append(currentBestGlobalPosition[i].ToString("G17"));
                sb.Append(",");
            }
            sb.Append(currentBestGlobalPosition[dimensions-1].ToString("G17"));
            Log.Instance().Append(sb.ToString());

            // minX
            sb.Clear();
            for (int i = 0; i < dimensions-1; ++i)
            {
                sb.Append(minX[i].ToString("G17"));
                sb.Append(",");
            }
            sb.Append(minX[dimensions-1].ToString("G17"));
            Log.Instance().Append(sb.ToString());

            // maxX
            sb.Clear();
            for (int i = 0; i < dimensions-1; ++i)
            {
                sb.Append(maxX[i].ToString("G17"));
                sb.Append(",");
            }
            sb.Append(maxX[dimensions-1].ToString("G17"));
            Log.Instance().Append(sb.ToString());

            Log.Instance().Close();


        }

        public static void Run(Func<double[], double> errorFunction, int particleCount, int dimensions, double[] minX, double[] maxX, int maxEpochs, 
            double minAcceptedError = 0.0, bool warmStart = true, string path = "pso.sav", int rndomSeed = 0)
        {

            double[] bestPosition = Solve(dimensions, particleCount, minX, maxX, maxEpochs, minAcceptedError, errorFunction, 
                out Particle[] finalSwarm, out int finalEpoch, out double minError, warmStart:warmStart, path:path, rndomSeed:rndomSeed);

        }

        static double[] Solve(int dimensions, int particleCount, double[] minX, double[] maxX, int maxEpochs, double minAcceptedError, 
            Func<double[], double> errorFunction, out Particle[] swarm, out int epoch, out double minError, bool warmStart = true, int rndomSeed = 0, string path="pso.sav")
        {
            Random random = new Random(rndomSeed);
            double magicMultiplier = 0.1; // TODO: why 0.1?

            swarm = new Particle[particleCount];
            double[] bestGlobalPosition = new double[dimensions];
            double minGlobalError = Double.MaxValue;
            
            if (warmStart && File.Exists(path))
            {
                Restore(out swarm, out epoch, out minGlobalError, out bestGlobalPosition, path: path);
            }
            else
            {
                for (int i = 0; i < swarm.Length; ++i) // Swarm initialization
                {
                    double[] randomPosition = new double[dimensions];
                    for (int j = 0; j < randomPosition.Length; ++j)
                        randomPosition[j] = (maxX[j] - minX[j]) * random.NextDouble() + minX[j];

                    double error = errorFunction(randomPosition);
                    double[] randomVelocity = new double[dimensions];

                    for (int j = 0; j < randomVelocity.Length; ++j)
                    {
                        double lo = minX[j] * magicMultiplier;
                        double hi = maxX[j] * magicMultiplier;
                        randomVelocity[j] = (hi - lo) * random.NextDouble() + lo;
                    }
                    swarm[i] = new Particle(randomPosition, error, randomVelocity, randomPosition, error);
                    
                    if (swarm[i].Error < minGlobalError) // Check if has global best position/solution
                    {
                        minGlobalError = swarm[i].Error;
                        swarm[i].Position.CopyTo(bestGlobalPosition, 0);
                    }
                }
                epoch = 0;
            }

            // Prepare
            double w = 0.729; // Inertia weight. see http://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=00870279
            double c1 = 1.49445; // Cognitive/local weight
            double c2 = 1.49445; // Social/global weight
            double deathProbability = 0.01;
            
            double[] newVelocity = new double[dimensions];
            double[] newPosition = new double[dimensions];
            
            while (epoch < maxEpochs && minGlobalError > minAcceptedError) // Main execution
            {
                
                foreach (Particle currentParticle in swarm)
                {
                    for (int j = 0; j < currentParticle.Velocity.Length; ++j) // New velocity
                        newVelocity[j] = (w * currentParticle.Velocity[j]) +
                                         (c1 * random.NextDouble() * (currentParticle.BestPosition[j] - currentParticle.Position[j])) + // Cognitive/Local weight randomization
                                         (c2 * random.NextDouble() * (bestGlobalPosition[j] - currentParticle.Position[j])); // Social/Global weight randomization

                    newVelocity.CopyTo(currentParticle.Velocity, 0);
                    
                    for (int j = 0; j < currentParticle.Position.Length; ++j) // Calculate new Position
                    {
                        newPosition[j] = currentParticle.Position[j] + newVelocity[j];
                        if (newPosition[j] < minX[j])
                            newPosition[j] = minX[j];
                        else if (newPosition[j] > maxX[j])
                            newPosition[j] = maxX[j];
                    }
                    newPosition.CopyTo(currentParticle.Position, 0);

                    double newError = errorFunction(newPosition);
                    currentParticle.Error = newError;

                    if (newError < currentParticle.BestError)
                    {
                        newPosition.CopyTo(currentParticle.BestPosition, 0);
                        currentParticle.BestError = newError;
                    }
                    if (newError < minGlobalError)
                    {
                        newPosition.CopyTo(bestGlobalPosition, 0);
                        minGlobalError = newError;
                    }
                    
                    if (random.NextDouble() < deathProbability) // If particle dies: new position, leave velocity, update error
                    {
                        for (int j = 0; j < currentParticle.Position.Length; ++j)
                            currentParticle.Position[j] = (maxX[j]  - minX[j]) * random.NextDouble() + minX[j];
                        currentParticle.Error = errorFunction(currentParticle.Position);
                        currentParticle.Position.CopyTo(currentParticle.BestPosition, 0);
                        currentParticle.BestError = currentParticle.Error;

                        if (currentParticle.Error < minGlobalError) // global best by chance?
                        {
                            minGlobalError = currentParticle.Error;
                            currentParticle.Position.CopyTo(bestGlobalPosition, 0);
                        }
                    }


                }
                ++epoch;

                Save(swarm, particleCount, dimensions, minX, maxX, epoch, minGlobalError, bestGlobalPosition);
                
            }
            
            double[] result = new double[dimensions];
            bestGlobalPosition.CopyTo(result, 0);
            minError = minGlobalError;

            return result;
        }

        public static double GetError(double[] x)
        {
            // 0.42888194248035300000 when x0 = -sqrt(2), x1 = 0
            double expectedMin = -0.42888194; // true min for z = x * exp(-(x^2 + y^2))
            double z = x[0] * Math.Exp(-((x[0] * x[0]) + (x[1] * x[1])));
            return Math.Pow(z - expectedMin, 2); // MSE
        }

        public static double GetError2(double[] x)
        {
            // Just a random function: -ax - b ^ 2 +  2 ^ x
            double expectedMin = -0.0861; // true min x,y = (0.5288, -0.0861)
            double z = -x[0] - 1 + Math.Pow(2, x[0]);
            return Math.Pow(z - expectedMin, 2); // MSE
        }

        /// <summary>
        /// Returns the Mean Square error for a solution to Himmelblau's function.
        /// <remarks>Min z = 0. Many global minima x,y = { (3, 2), (-2.805, -3.28), (-3.779, -3.283), (3.584, -1.848) }.
        /// Wiki info: https://en.wikipedia.org/wiki/Himmelblau%27s_function. </remarks>
        /// </summary>
        /// <param name="xArray">Proposed solution 2d array for input values.</param>
        /// <returns>Mean Squared Error, double value.</returns>
        public static double HimmelblauMse(double[] xArray)
        {
            double expectedMin = 0; // Min z = 0. Many global minima x,y = { (3, 2), (-2.805, -3.28), (-3.779, -3.283), (3.584, -1.848) }
            double x = xArray[0];
            double y = xArray[1];
            double z = Math.Pow(Math.Pow(x, 2) + y - 11, 2) + Math.Pow(x + Math.Pow(y, 2) - 7, 2);
            return Math.Pow(z - expectedMin, 2); // MSE
        }

        /// <summary>
        /// Returns the Mean Squared Error for a solution to Michalewicz's function for 2 dimensions. 
        /// <remarks>Has factorial dimension (d!) local minima. Parameter m (default: 10), defines the steepness of the minima.
        /// For 2 dimensions, min z = -1.8013, global minima: x,y = (2.20, 1.57) with x_i in (0, PI).
        /// More info: https://www.sfu.ca/~ssurjano/michal.html </remarks>
        /// </summary>
        /// <param name="xArray">Proposed solution 2d array for input values.</param>
        /// <returns>Mean Squared Error, double value.</returns>
        public static double MichalewiczMse2(double[] xArray)
        {
            int d = 2; // Dimensions
            double expectedMin = -1.8013; // For 2 dimensions, min z = -1.8013. Global minima x,y = (2.20, 1.57) with x_i in (0, PI)
            int m = 10; // constant with default value 10

            double sum = 0;
            for (int i = 1; i <= d; i++)
            {
                double xi = xArray[i - 1];
                sum += Math.Sin(xi) * Math.Pow(Math.Sin(i * Math.Pow(xi, 2) / Math.PI), 2 * m);
            }
            double z = -sum;

            return Math.Pow(z - expectedMin, 2); // MSE
        }

        public static double MichalewiczMse5(double[] xArray)
        {
            int d = 5; // Dimensions
            double expectedMin = -4.687658; // For 5 dimensions, min z = -4.687658 with x_i in (0, PI)
            int m = 10; // constant with default value 10

            double sum = 0;
            for (int i = 1; i <= d; i++)
            {
                double xi = xArray[i - 1];
                sum += Math.Sin(xi) * Math.Pow(Math.Sin(i * Math.Pow(xi, 2) / Math.PI), 2 * m);
            }
            double z = -sum;

            return Math.Pow(z - expectedMin, 2); // MSE
        }
        
        public static double EggHolder(double[] xArray)
        {
            // See: https://www.sfu.ca/~ssurjano/egg.html
            double expectedMin = -959.6407; // Min z = -959.6407, with x,y = (512, 404.2319) in [-512, 512]
            double x = xArray[0];
            double y = xArray[1];
            double z = -(y + 47) * Math.Sin(Math.Sqrt(Math.Abs(y + x / 2 + 47))) - x * Math.Sin(Math.Sqrt(Math.Abs(x - (y + 47))));
            return Math.Pow(z - expectedMin, 2); // MSE
        }

        public static double MishraSBird(double[] xArray)
        {
            // See: https://en.wikipedia.org/wiki/Test_functions_for_optimization
            double expectedMin = -106.7645367; // Global Min z = -106.7645367 with x,y = (-3.31302468, -1.5821422). Search domain: x in [-10, 0] and y in [-6.5, 0]
            double x = xArray[0];
            double y = xArray[1];

            if (x < -10 || y < -6.5 || x > 0 || y > 0) // Search domain constraints: x in [-10, 0] and y in [-6.5, 0]
                return double.MaxValue;

            double subjectedTo = Math.Pow(x + 5, 2) + Math.Pow(y + 5, 2);
            double lessThanValue = 25;
            if (subjectedTo >= lessThanValue) // Subjected to constraint
                return double.MaxValue;
            
            double z = Math.Sin(y) * Math.Pow(Math.E, Math.Pow(1 - Math.Cos(x), 2)) + Math.Cos(x) * Math.Pow(Math.E, Math.Pow(1 - Math.Sin(y), 2)) + Math.Pow(x - y, 2);
            return Math.Pow(z - expectedMin, 2); // MSE
        }

        public static double Townsend(double[] xArray)
        {
            // See: https://en.wikipedia.org/wiki/Test_functions_for_optimization
            double expectedMin = -2.0239884; // Global Min z = -2.0239884 with x,y = (2.0052938, 1.1944509). Search domain: x in [-2.25, 2.5] and y in [-2.5, 1.75]
            double x = xArray[0];
            double y = xArray[1];

            if (x < -2.25 || y < -2.5 || x > 2.5 || y > 1.75) // Search domain constraints: x in [-2.25, 2.5] and y in [-2.5, 1.75]
                return double.MaxValue;

            double subjectedTo = Math.Pow(x, 2) + Math.Pow(y, 2);
            double t = Math.Atan2(x, y);

            double lessThanValue = Math.Pow(
                2 * Math.Cos(t) - (1 / (double)2) * Math.Cos(2 * t) - (1/(double)4) * Math.Cos(3 * t) - (1/(double)8) * Math.Cos(4 * t)
                , 2) + Math.Pow(2 * Math.Sin(t), 2);
            if (subjectedTo >= lessThanValue) // Subjected to constraint
                return double.MaxValue;
            
            double z = -Math.Pow(Math.Cos((x - 0.1) * y) , 2) - x * Math.Sin(3 * x + y);
            return Math.Pow(z - expectedMin, 2); // MSE
        }
    }
}
