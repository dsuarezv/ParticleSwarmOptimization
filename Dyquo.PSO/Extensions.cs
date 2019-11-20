using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyqui.PSO
{
    public static class Extensions
    {
        public static double[] Clone2(this double[] source)
        {
            var result = new double[source.Length];
            source.CopyTo(result, 0);
            return result;
        }



        public static double WeightedAverage<T>(this IEnumerable<T> records, Func<T, double> value, Func<T, double> weight)
        {
            double weightedValueSum = records.Sum(x => value(x) * weight(x));
            double weightSum = records.Sum(x => weight(x));

            if (weightSum != 0)
                return weightedValueSum / weightSum;
            else
                throw new DivideByZeroException("The sum of weights equals 0.");
        }

    }
}
