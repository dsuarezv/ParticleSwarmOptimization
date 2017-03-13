using Dyquo.Charts3d;
using Dyquo.Optimization.Swarm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Dyquo.Optimization.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ParticlesViewModel mParticles;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;

            await RunSolver();

            RunButton.IsEnabled = true;
        }


        private async Task RunSolver()
        {
            var solverOptions = new PSOSolverOptions
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

                // Slower convergence params for visualization

                //VelocityInitialAtenuation = 0.01
                //InertiaWeight = 0.02,
                //C1CognitiveWeight = 0.05,
                //C2SocialWeight = 0.05,
            };

            var solver = new PSOSolver(ErrorFunction, solverOptions);
            var data = solver.Initialize();

            if (mParticles == null)
            {
                SetupFunctionPlot(data);
                mParticles = SetupParticlesPlot(data);
                Show3d.ZoomExtents();
            }
            

            solver.AfterEpoch += (s, d) =>
            {
                Dispatcher.Invoke(() =>
                {
                    mParticles.UpdateParticles(d.Particles);
                    mParticles.UpdateBest(d.BestGlobalPosition, Function2d(d.BestGlobalPosition));
                    UpdateStatusLabels(d);
                });

                Task.Delay(30).Wait();
            };

            StatusLabel.Content = "Particle Swarm Optimizer Running";

            var res = await solver.SolveAsync(data);

            StatusLabel.Content = "Solver finished";
        }

        private void SetupFunctionPlot(PSOSolverData data)
        {
            var surface = new Surface3dViewModel()
            {
                Function = Function2d,
                MinX = -3,
                MinY = -3,
                MaxX = 3,
                MaxY = 3
            };

            Show3d.AddSurface(surface);
        }

        private ParticlesViewModel SetupParticlesPlot(PSOSolverData data)
        {
            var result = new ParticlesViewModel();
            Show3d.AddContent(result.SetupParticles(data));

            return result;
        }

        private void UpdateStatusLabels(PSOSolverData data)
        {
            CycleInfoLabel.Content = $"Cycle no. {data.Epoch}";
            BestStateLabel.Content = $"Solution at ({GetPositionString(data.BestGlobalPosition)})\nFunction value at position {Function2d(data.BestGlobalPosition)}"; // "\nError {data.BestGlobalError.ToString("F16")}";
        }

        private string GetPositionString(double[] pos)
        {
            return $"{pos[0].ToString("F8")}, {pos[1].ToString("F8")}";
        }

        // __ Function to optimize ____________________________________________


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
    }
}
