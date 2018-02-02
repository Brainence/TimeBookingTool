using System;
using TBT.App.Models.AppModels;

namespace TBT.App.Helpers
{
    public interface ICacheable: IDisposable
    {
        DateTime ExpiresDate { get; set; }
        void OpenTab(User currentUser);
        void CloseTab();
    }
}
