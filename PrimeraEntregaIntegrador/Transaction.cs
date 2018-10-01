using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeraEntregaIntegrador
{
    class Transaction
    {

        public SortedSet<String> items;
        public String code;
        public String clientCode;
        public DateTime date;


        public Transaction(String code,String clientCode,DateTime date)
        {
            this.code = code;
            this.clientCode = clientCode;
            this.date = date;
            items = new SortedSet<string>();
        }

        public Transaction(String code, String clientCode, DateTime date,String item)
        {
            this.code = code;
            this.clientCode = clientCode;
            this.date = date;
            items = new SortedSet<string>();
            items.Add(item);
        }
    }
}
