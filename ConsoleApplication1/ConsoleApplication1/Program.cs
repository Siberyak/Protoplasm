using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApplication1.TestData;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            int r1;
            Math.DivRem((int)DayOfWeek.Sunday+7, 7, out r1);

            int r2;
            Math.DivRem((int)DayOfWeek.Monday + 7, 7, out r2);

            int r3;
            Math.DivRem((int)DayOfWeek.Friday + 7, 7, out r3);

            _TestDataLoad.Do();
        }
    }
}
