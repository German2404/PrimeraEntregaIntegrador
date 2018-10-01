using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeraEntregaIntegrador
{
    class Association
    {

        public SortedSet<String> from;
        public SortedSet<String> to;

        public Association(SortedSet<string> from, SortedSet<string> to)
        {
            this.from = from;
            this.to = to;
        }

        override
        public String ToString()
        {
            String a = "{" + String.Join(",", from) + "}";
            String b = "{" + String.Join(",", to) + "}";
            return a + "->" + b
;        }


        public override bool Equals(object obj)
        {
            var obj1 = obj as Association;
            return this.from.SetEquals(obj1.from) && this.to.SetEquals(obj1.to);
        }


        public override int GetHashCode()
        {
            return this.from.GetHashCode() + this.to.GetHashCode();
        }
    }
}
