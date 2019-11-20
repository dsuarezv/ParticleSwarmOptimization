using Dyquo.Charts3d;
using Dyqui.PSO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Dyquo.Optimization.UI
{
    public class ParticlesViewModel
    {
        public Model3DGroup SetupParticles(SolverData data)
        {
            mParticleModels.Clear();

            var result = new Model3DGroup();

            foreach (var p in data.Particles)
            {
                var cube = Models.GetCube(new SolidColorBrush(Colors.Orange));
                UpdateParticlePosition(cube, 0, 0, mDefaultParticleZ);

                mParticleModels.Add(cube);
                result.Children.Add(cube);
            }

            mBestSolution = Models.GetCube(new SolidColorBrush(Colors.Red));
            UpdateParticlePosition(mBestSolution, 0, 0, mDefaultParticleZ);
            result.Children.Add(mBestSolution);

            return result;
        }

        public void UpdateParticles(IList<Particle> particles)
        {
            for (int i = 0; i < particles.Count; ++i)
            {
                var p = particles[i];
                var x = p.Position[0];
                var y = p.Position[1];
                UpdateParticlePosition(mParticleModels[i], x, y, mDefaultParticleZ);
            }
        }

        public void UpdateBest(double[] pos, double z)
        {
            UpdateParticlePosition(mBestSolution, pos[0], pos[1], z);
        }



        private void UpdateParticlePosition(Model3D model, double x, double y, double z)
        {
            var size = mParticleSize;

            model.Transform = Models.GetTransform(new Point3D(x, y, z), new Size3D(size, size, size));
        }



        private List<Model3D> mParticleModels = new List<Model3D>();
        private Model3D mBestSolution;
        private double mDefaultParticleZ = 0.5;
        private double mParticleSize = 0.03;
        
    }
}
