using System;

namespace TBT.App.Helpers
{
    public interface ICacheable: IDisposable
    {
        DateTime ExpiresDate { get; set; }
    }
}
