using System.Threading.Tasks;
using TBT.App.Models.AppModels;

namespace TBT.App.Services.CommunicationService.Interfaces
{
    public interface ICommunicationService
    {
        Task<string> GetAsJson(string url, bool allowAnonymous);
        Task<string> PostAsJson(string url, object data, bool allowAnonymous);
        Task<string> PutAsJson(string url, object data, bool allowAnonymous);
        Task<string> Delete(string url, bool allowAnonymous);
    }
}
