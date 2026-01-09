using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DigitalCap.Core.Models;

namespace DigitalCap.Core.Interfaces.Repository
{

    public interface IPlatformUserRepository
    {
        Task DeleteUserWCNMappingAsync(string email);

        Task AddEditUserWCNMappingAsync(string email, string companyName, UserWCNMappingModel model);

        Task<IEnumerable<UserWCNMappingModel>> GetWCNGridAsync(string email);

        Task PerformInitialLoginAsync(string email);

        Task<bool> AcceptTermsOfUseAsync(Guid userId,int applicationId,bool termsAccepted,string performedBy);

        Task<string?> GetTermsOfUseTextAsync(int applicationId);
        Task<UserModel?> GetUserTermsAsync(string userId, int applicationId);

        Task<bool> AddNewWCNAsync(string email,WCNModel model);
        Task<bool> AcceptTermsOfUseAsync(UserModel userModel, int applicationId, string loggedInUserName);
        Task AddEditUserWCNMappingAsync(string email, string companyName, int wCN);
        Task<UserModel> GetUser(int appId, string loggedInUserName, string loggedInUserId);
        //Task AddEditUserWCNMappingAsync(string email, string companyName, int wCN);
        //Task<bool> AcceptTermsOfUseAsync(UserModel userModel, int applicationId, string loggedInUserName);
        //Task<UserModel> GetUser(int appId, string loggedInUserName, string userId);
        //Task<bool> AddNewWCN(WCNModel wCNModel, string loggedInUserName);
    }
}






