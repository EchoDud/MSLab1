using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLab1
{
    internal class Сalculations
    {
        public static double NormalQuantile(double interval)
        {
            return 4.91 * ((Math.Pow(interval, 0.14)) - Math.Pow((1 - interval), 0.14));
        }
    }
}
