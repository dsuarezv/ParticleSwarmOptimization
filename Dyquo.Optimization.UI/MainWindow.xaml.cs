using Dyquo.Charts3d;
using Dyqui.PSO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Reactive;
namespace Dyquo.Optimization.UI
{
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Windows.Input;

    using Reactive.Bindings;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ParticlesViewModel particles;
        private static readonly ISubject<bool> canExecute = new Subject<bool>();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            (RunCommand as ReactiveCommand).Subscribe(async a => { canExecute.OnNext(false); await RunSolver(); canExecute.OnNext(true); });
        }

        public ICommand RunCommand { get; } = new ReactiveCommand(canExecute, true);


        public ReactiveProperty<string> Status { get; } = canExecute
                                                        .Select(b => b ? "finished" : "running")
                                                        .StartWith("ready")
                                                        .ToReactiveProperty();


        private void SetUpUI(SolverData data)
        {
            if (this.particles == null)
            {
                SetupFunctionPlot(data);
                this.particles = SetupParticlesPlot(data);
                Show3d.ZoomExtents();
            }
        }

        private async Task RunSolver()
        {
            var solver = new Solver(ErrorFunction, GetPsoSolverOptions());
            var data = solver.Initialize();
            this.SetUpUI(data);

            solver.AfterEpoch += (s, d) =>
           {
               Dispatcher.Invoke(() =>
               {
                   this.particles.UpdateParticles(d.Particles);
                   this.particles.UpdateBest(d.BestGlobalPosition, Function2d(d.BestGlobalPosition));
                   UpdateStatusLabels(d);
               });

               Task.Delay(30).Wait();
           };

            var res = await solver.SolveAsync(data);
        }



        private SolverOptions GetPsoSolverOptions() => new SolverOptions
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

        private void SetupFunctionPlot(SolverData data)
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

        private ParticlesViewModel SetupParticlesPlot(SolverData data)
        {
            var result = new ParticlesViewModel();
            Show3d.AddContent(result.SetupParticles(data));
            return result;
        }

        private void UpdateStatusLabels(SolverData data)
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
