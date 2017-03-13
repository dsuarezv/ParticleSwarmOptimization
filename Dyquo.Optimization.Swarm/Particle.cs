using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyquo.Optimization.Swarm
{
    public class Particle
    {
        public double[] Position;
        public double[] Velocity;
        public double[] BestPosition;
        public double Error;
        public double BestError;


        public Particle(double[] position, double[] velocity, double[] bestPosition, double err, double bestError)
        {
            Position = position.Clone2();
            Velocity = velocity.Clone2();
            BestPosition = bestPosition.Clone2();
            
            BestError = bestError;
            Error = err;
        }
    }
}
