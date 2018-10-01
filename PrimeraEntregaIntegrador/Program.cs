using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeraEntregaIntegrador
{
    class Program
    {
        static void Main(string[] args)
        {

            for (int i = 1; i < 15; i+=1)
            {
                
            Analyzer a = new Analyzer(0, 0);
            a.readTransactions("./"+i+".txt");
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var assoBF = a.giveBruteForceRefinedAssotiations(0, 100, 0, 100);
            Console.WriteLine("BF ("+i+"):"+timer.Elapsed.TotalMilliseconds);
                var timer2 = System.Diagnostics.Stopwatch.StartNew();
                var assoAP = a.giveAPrioriRefinedAssotiations(0, 100, 0, 100);
                Console.WriteLine("AP (" + i + "):" + timer2.Elapsed.TotalMilliseconds+"\n");

            }


           
         
        }
    }
}
