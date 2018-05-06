using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4.ViewModels
{
    class PlotHelper
    {
        public static List<Func<double, double>> GetFunctions()
        {
            return new List<Func<double, double>> {
                (arg) => Calc0(arg),
                (arg) => Calc1(arg),
                (arg) => Calc2(arg),
                (arg) => Calc3(arg),
                (arg) => Calc4(arg),
                (arg) => Calc5(arg),
                (arg) => Calc6(arg),
                (arg) => Calc7(arg),
            };
        }

        private static double Calc0(double arg)
        {
            //return 8 * Math.Pow(arg, 2);
            return 2 * Math.Pow(arg, 2);
        }

        private static double Calc1(double arg)
        {
            //return 7 * Math.Pow(arg, 2);
            return 1 * Math.Pow(arg, 2);
        }

        private static double Calc2(double arg)
        {
            return 6 * Math.Pow(arg, 2);
        }

        private static double Calc3(double arg)
        {
            return 5 * Math.Pow(arg, 2);
        }

        private static double Calc4(double arg)
        {
            return 4 * Math.Pow(arg, 2);
        }

        private static double Calc5(double arg)
        {
            return 3 * Math.Pow(arg, 2);
        }

        private static double Calc6(double arg)
        {
            return 2 * Math.Pow(arg, 2);
        }

        private static double Calc7(double arg)
        {
            return 1 * Math.Pow(arg, 2);
        }
    }
}
