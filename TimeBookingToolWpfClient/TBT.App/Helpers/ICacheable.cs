using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBT.App.Helpers
{
    public interface ICacheable: IDisposable
    {
        DateTime ExpiresDate { get; set; }
    }
}
