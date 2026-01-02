using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Security;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Infrastructure.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Infrastructure.Service
{
    public class UserAccountService : IUserAccountService
    {
        private readonly IUserAccountRepository _repository;

        public UserAccountService(IUserAccountRepository repository)
        {
            _repository = repository;
        }

        // -------------------- Queries --------------------

        public async Task<UserAccountModel> GetByAspNetIdAsync(string aspNetUserId)
        {
            return await _repository.GetByAspNetIdAsync(aspNetUserId);
        }

        public async Task<UserAccountModel> GetByAspNetIdIncludingDeletedAsync(string aspNetUserId)
        {
            return await _repository.GetByAspNetIdIncludingDeletedAsync(aspNetUserId);
        }

        public async Task<IEnumerable<UserAccountModel>> GetUsersInRoleAsync(string roleName)
        {
            return await _repository.GetUsersInRoleAsync(roleName);
        }

        public async Task<IEnumerable<UserAccountModel>> GetUsersInRoleForClientAsync(string roleName, Guid clientId)
        {
            return await _repository.GetUsersInRoleForClientAsync(roleName, clientId);
        }

        public async Task<IEnumerable<UserAccountModel>> GetUsersWithPermissionAsync(Permission permission)
        {
            return await _repository.GetUsersWithPermissionAsync( permission);
        }

        public async Task<IEnumerable<CapUser>> GetAbsUsersAsync()
        {
            return await _repository.GetAbsUsersAsync();
        }

        public async Task<IEnumerable<CapUser>> GetClientUsersAsync(string clientId)
        {
            return await _repository.GetClientUsersAsync(clientId);
        }

        // -------------------- Commands --------------------

        public async Task<UserAccountModel> SaveAsync(UserAccountModel account)
        {
            var existing =
                await _repository.GetByAspNetId_ActiveOrDeleted(account.AspNetUserId);

            if (existing == null)
            {
                _repository.Insert(account);
            }
            else
            {
                account.Id = existing.Id;
                _repository.Update(account);
            }

            return account;
        }

        public async Task CommitAsync()
        {
            await Task.Run(() => _repository.Commit());
        }
    }
}





    


