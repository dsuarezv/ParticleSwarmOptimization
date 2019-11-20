using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyqui.PSO
{
    public class SolverOptions
    {
        public int NumDimensions;
        public int NumParticles;
        public double MinimumX;
        public double MaximumX;
        public int MaxEpochs;
        public double AcceptanceError;

        public double InertiaWeight = 0.729;
        public double C1CognitiveWeight = 1.49445;
        public double C2SocialWeight = 1.49445;

        public double VelocityInitialAtenuation = 0.1;

        public double ParticleResetProbability = 0.0;

        public int RandomSeed = 0;
    }
}
