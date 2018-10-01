using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeraEntregaIntegrador
{
    class PowerSetGenerator
    {


        public static T[][] FastPowerSet<T>(T[] seq)
        {
            var powerSet = new T[1 << seq.Length][];
           
            powerSet[0] = new T[0]; // starting only with empty set
            for (int i = 0; i < seq.Length; i++)
            {
                
                    
                
                var cur = seq[i];
                int count = 1 << i; // doubling list each time
                for (int j = 0; j < count; j++)
                {
                   
                    var source = powerSet[j];
                    var destination = powerSet[count + j] = new T[source.Length + 1];
                    for (int q = 0; q < source.Length; q++)
                        destination[q] = source[q];
                    destination[source.Length] = cur;
                }
            }
            var lista = powerSet.Select(i => i).Where(i => i.Length > 0).ToArray();
            return lista;
        }
    }
}
