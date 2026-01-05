using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Persistence.Repositories
{
    public class ClientRepository : RepositoryBase<Client, Guid>, IClientRepository
    {
        private readonly ILogger<ProjectRepository> _logger;
        public ClientRepository(IUnitOfWork unitOfWork, ILogger<ProjectRepository> logger) : base(unitOfWork, logger)
        {
            _logger = logger;
        }
    }
}
