using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface ISecurityClientService
    {
        Task<bool> ClientNameExistsAsync(string clientName);

        Task<IReadOnlyList<ClientModel>> GetAllClientsAsync();

        Task<ClientModel> CreateClientAsync(ClientModel model);
    }
}

