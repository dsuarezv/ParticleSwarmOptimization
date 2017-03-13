using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyquo.Optimization.Swarm
{
    public class PSOSolver
    {
        public event EpochDelegate AfterEpoch;


        public PSOSolver(ErrorFunctionDelegate errFunc, PSOSolverOptions options)
        {
            mErrorFunction = errFunc;
            mOptions = options;
            mRandom = new Random(options.RandomSeed);
        }

        public PSOSolverData Initialize()
        {
            var data = new PSOSolverData(mOptions.NumParticles, mOptions.NumDimensions);

            // swarm initialization
            for (int i = 0; i < data.Particles.Length; ++i)
            {
                double[] randomPosition = new double[mOptions.NumDimensions];
                for (int j = 0; j < randomPosition.Length; ++j)
                {
                    randomPosition[j] = GetRandomPosition();
                }

                double error = mErrorFunction(randomPosition);
                double[] randomVelocity = new double[mOptions.NumDimensions];

                for (int j = 0; j < randomVelocity.Length; ++j)
                {
                    double lo = mOptions.MinimumX * mOptions.VelocityInitialAtenuation;
                    double hi = mOptions.MaximumX * mOptions.VelocityInitialAtenuation;
                    randomVelocity[j] = (hi - lo) * mRandom.NextDouble() + lo;
                }

                data.Particles[i] = new Particle(randomPosition, randomVelocity, randomPosition, error, error);

                // global best position/solution?
                if (data.Particles[i].Error < data.BestGlobalError)
                {
                    data.BestGlobalError = data.Particles[i].Error;
                    data.Particles[i].Position.CopyTo(data.BestGlobalPosition, 0);
                }
            }

            return data;
        }


        public async Task<PSOResult> SolveAsync(PSOSolverData data)
        {
            return await Task.Run(() => Solve(data));
        }
        

        public PSOResult Solve(PSOSolverData data)
        {
            double[] newVelocity = new double[mOptions.NumDimensions];
            double[] newPosition = new double[mOptions.NumDimensions];
            double newError;
            var result = new PSOResult();

            while (data.Epoch < mOptions.MaxEpochs)
            {
                foreach (var particle in data.Particles)
                {
                    // Update velocity
                    for (int j = 0; j < mOptions.NumDimensions; ++j)
                    {
                        double r1 = mRandom.NextDouble(); // cognitive and social randomizations
                        double r2 = mRandom.NextDouble();

                        newVelocity[j] = (mOptions.InertiaWeight * particle.Velocity[j]) +
                            (mOptions.C1CognitiveWeight * r1 * (particle.BestPosition[j] - particle.Position[j])) +
                            (mOptions.C2SocialWeight * r2 * (data.BestGlobalPosition[j] - particle.Position[j]));
                    }

                    newVelocity.CopyTo(particle.Velocity, 0);


                    // Update position
                    for (int j = 0; j < mOptions.NumDimensions; ++j)
                    {
                        newPosition[j] = particle.Position[j] + newVelocity[j];

                        if (newPosition[j] < mOptions.MinimumX)
                        {
                            newPosition[j] = mOptions.MinimumX;
                        }
                        else if (newPosition[j] > mOptions.MaximumX)
                        {
                            newPosition[j] = mOptions.MaximumX;
                        }
                    }

                    newPosition.CopyTo(particle.Position, 0);


                    // Check error of new position and update if improved
                    newError = mErrorFunction(newPosition);
                    particle.Error = newError;

                    if (newError < particle.BestError)
                    {
                        newPosition.CopyTo(particle.BestPosition, 0);
                        particle.BestError = newError;
                    }

                    if (newError < data.BestGlobalError)
                    {
                        newPosition.CopyTo(data.BestGlobalPosition, 0);
                        data.BestGlobalError = newError;
                    }


                    // death?
                    double die = mRandom.NextDouble();

                    if (die < mOptions.ParticleResetProbability)
                    {
                        for (int j = 0; j < mOptions.NumDimensions; ++j)
                        {
                            particle.Position[j] = GetRandomPosition();
                        }

                        particle.Error = mErrorFunction(particle.Position);
                        particle.BestError = particle.Error;
                        particle.Position.CopyTo(particle.BestPosition, 0);

                        if (particle.Error < data.BestGlobalError)
                        {
                            data.BestGlobalError = particle.Error;
                            particle.Position.CopyTo(data.BestGlobalPosition, 0);
                        }
                    }

                } 

                AfterEpoch?.Invoke(this, data);

                ++data.Epoch;

                if (data.BestGlobalError < mOptions.AcceptanceError)
                {
                    result.Success = true;
                    break;
                }
            }
            
            result.BestPosition = data.BestGlobalPosition.Clone2();
            result.BestError = data.BestGlobalError;

            return result;
        }

        private double GetRandomPosition()
        {
            return (mOptions.MaximumX - mOptions.MinimumX) * mRandom.NextDouble() + mOptions.MinimumX;
        }

        private ErrorFunctionDelegate mErrorFunction;
        private PSOSolverOptions mOptions;
        private Random mRandom;
    }


    public delegate double ErrorFunctionDelegate(double[] values);
    public delegate void EpochDelegate(PSOSolver sender, PSOSolverData data);
}



