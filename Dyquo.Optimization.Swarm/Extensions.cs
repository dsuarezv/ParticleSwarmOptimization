using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyquo.Optimization.Swarm
{
    public static class Extensions
    {
        public static double[] Clone2(this double[] source)
        {
            var result = new double[source.Length];
            source.CopyTo(result, 0);
            return result;
        }
    }
}
