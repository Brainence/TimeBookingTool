using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBT.App.Models.Tools
{
    class ApiRate
    {
        public string Ccy { get; set; }
        public string BaseCcy { get; set; }
        public decimal Buy { get; set; }
        public decimal Sale { get; set; }
    }
}
