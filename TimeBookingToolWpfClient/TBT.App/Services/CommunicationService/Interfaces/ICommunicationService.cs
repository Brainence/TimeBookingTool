﻿using System.Threading.Tasks;
using TBT.App.Models.AppModels;

namespace TBT.App.Services.CommunicationService.Interfaces
{
    public interface ICommunicationService
    {
        Task<string> GetAsJson(string url);
        Task<string> PostAsJson(string url, object data);
        Task<string> PutAsJson(string url, object data);
        Task<string> Delete(string url);
    }
}
