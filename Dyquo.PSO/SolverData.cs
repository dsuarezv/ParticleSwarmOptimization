using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyqui.PSO
{
    public class SolverData
    {
        public Particle[] Particles;
        public double[] BestGlobalPosition;
        public double BestGlobalError;
        public int Epoch = 0;

        public SolverData(int numParticles, int numDimensions)
        {
            Particles = new Particle[numParticles];
            BestGlobalPosition = new double[numDimensions];
            BestGlobalError = double.MaxValue;
        }
    }
}
