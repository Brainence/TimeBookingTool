using System.Threading;
using System.Windows.Data;

namespace TBT.App.Models.Tools
{
    public class CultureAwareBinding : Binding
    {
        public CultureAwareBinding()
        {
            ConverterCulture = Thread.CurrentThread.CurrentUICulture;
        }
    }
}
