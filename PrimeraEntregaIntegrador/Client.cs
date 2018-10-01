using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeraEntregaIntegrador
{
    class Client
    {

        public String code;
        public String group;
        public String city;
        public String department;
        public String paymentGroup;

        public Client(string code, string group, string city, string department, string paymentGroup)
        {
            this.code = code;
            this.group = group;
            this.city = city;
            this.department = department;
            this.paymentGroup = paymentGroup;
        }
    }
}
