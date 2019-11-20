
namespace Dyqui.ConsoleDemo
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using Dyqui.PSO;

    class Program
    {
        static void Main(string[] args)
        {
            RunSolver();
            Console.ReadLine();
        }

        private static async void RunSolver()
        {
            var solver = new Solver(ErrorFunction, GetPsoSolverOptions());
            var data = solver.Initialize();

            GetParticleUpdates(solver)
                .Subscribe(particles =>
                {
                    var result = particles.Where(p => p.Error != 0).Select(p => (xy: p.BestPosition, score: 1 / p.Error)).ToArray();
                    var x = result.WeightedAverage(a => a.xy[0], a => a.score);
                    var y = result.WeightedAverage(a => a.xy[1], a => a.score);
                    Console.WriteLine($"{x},{y}");
                });
            
            _ = await solver.SolveAsync(data);
        }


        static IObservable<Particle[]> GetParticleUpdates(Solver solver)
        {
            return Observable.FromEvent<EpochDelegate, SolverData>( 
                    onNextHandler => (s,  d) => onNextHandler(d),
                    d => solver.AfterEpoch += d, 
                    d => solver.AfterEpoch -= d)
                .Select(d =>d.Particles);


        }


        static double ErrorFunction(double[] pos)
        {
            double target = -0.42888194;
            //double target = -100000;        // To minimize without specific target

            double f = Function2d(pos);

            // Quadratic error
            return (f - target) * (f - target);
        }

        static double Function2d(double x, double y)
        {
            return x * Math.Exp(-((x * x) + (y * y)));
        }

        static double Function2d(double[] pos)
        {
            var x = pos[0];
            var y = pos[1];
            return Function2d(x, y);
        }

        private static SolverOptions GetPsoSolverOptions() => new SolverOptions
        {
            NumDimensions = 2,
            NumParticles = 50,
            MaxEpochs = 500,
            MinimumX = -10.0,
            MaximumX = 10.0,
            AcceptanceError = 0.00000000,
            InertiaWeight = 0.729,
            C1CognitiveWeight = 1.5,
            C2SocialWeight = 1.5,
            ParticleResetProbability = 0.001,
        };
    }
}
