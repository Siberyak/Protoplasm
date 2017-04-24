using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApplication1.TestData;
using ConsoleApplication1.TestData.Messaging;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            MessagingTests.AkkaTest();

            Tests.Do();
        }
    }
}
