using DigitalCap.Core.Enumerations;
using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.WebApi.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Data;

namespace DigitalCap.Persistence.Repositories
{
    public class PlatformUserRepository :IPlatformUserRepository
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly UserManager<ApplicationDto> _userManager;
        public async Task<string> GetLoggedInUserName()
        {
            ClaimsPrincipal principal = _httpContext.HttpContext.User as ClaimsPrincipal;
            var userId = _userManager.GetUserId(principal);
            if (userId == null) return string.Empty;
            var user = await _userManager.FindByIdAsync(userId);
            return user.UserName;
        }

        public async Task<IEnumerable<UserType>> GetUserType(string objectId)
        {
            ClaimsPrincipal principal = _httpContext.HttpContext.User as ClaimsPrincipal;
            var userId = _userManager.GetUserId(principal);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<UserType>();
            }
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var ret = roles.ConvertAll(x =>
            {
                return (UserType)Enum.Parse(typeof(UserType), x.ToUpper());
            });
            return ret;
        }
        //private new readonly ILogger<PlatformUserRepository> _logger;

        //public PlatformUserRepository(
        //    IUnitOfWork unitOfWork,
        //    ILogger<PlatformUserRepository> logger)
        //    : base(unitOfWork, logger)
        //{
        //    _logger = logger;
        //}

        //#region User WCN Mapping

        //public async Task DeleteUserWCNMappingAsync(string email)
        //{
        //    await Connection.ExecuteAsync(
        //        sql: "[CAP].[Delete_User_WCN_Mapping]",
        //        param: new { EmailAddress = email },
        //        commandType: CommandType.StoredProcedure,
        //        transaction: Transaction);
        //}

        //public async Task AddEditUserWCNMappingAsync(string email, string companyName, UserWCNMappingModel model)
        //{
        //    var parameters = new DynamicParameters();
        //    parameters.Add("@EmailAddress", email);
        //    parameters.Add("@CompanyName", model.CompanyName);
        //    parameters.Add("@WCN", model.WCN);

        //    await Connection.ExecuteAsync(
        //        sql: "[CAP].[AddEdit_User_WCN_Mapping]",
        //        param: parameters,
        //        commandType: CommandType.StoredProcedure,
        //        transaction: Transaction);
        //}

        //public async Task<IEnumerable<UserWCNMappingModel>> GetWCNGridAsync(string email)
        //{
        //    return await Connection.QueryAsync<UserWCNMappingModel>(
        //        sql: "[CAP].[Get_WCN_Grid]",
        //        param: new { EmailAddress = email },
        //        commandType: CommandType.StoredProcedure,
        //        transaction: Transaction);
        //}

        //#endregion

        //#region Login & Terms

        //public async Task PerformInitialLoginAsync(string email)
        //{
        //    await Connection.ExecuteAsync(
        //        sql: "[CAP].[Perform_Initial_Login]",
        //        param: new { EmailAddress = email },
        //        commandType: CommandType.StoredProcedure,
        //        transaction: Transaction);
        //}

        //public async Task<bool> AcceptTermsOfUseAsync(Guid userId,int applicationId,bool termsAccepted,string performedBy)
        //{
        //    var parameters = new DynamicParameters();
        //    parameters.Add("@UserId", userId);
        //    parameters.Add("@ApplicationId", applicationId);
        //    parameters.Add("@TermsAccepted", termsAccepted);
        //    parameters.Add("@User", performedBy);

        //    var result = await Connection.ExecuteScalarAsync<int>(
        //        sql: "[CAP].[AddEdit_Terms_Of_Use]",
        //        param: parameters,
        //        commandType: CommandType.StoredProcedure,
        //        transaction: Transaction);

        //    return result > 0;
        //}

        //public async Task<string?> GetTermsOfUseTextAsync(int applicationId)
        //{
        //    return await Connection.ExecuteScalarAsync<string>(
        //        sql: "[CAP].[Get_Terms_Of_Use_Text]",
        //        param: new { ApplicationId = applicationId },
        //        commandType: CommandType.StoredProcedure,
        //        transaction: Transaction);
        //}

        //public async Task<UserModel?> GetUserTermsAsync(string userId, int applicationId)
        //{
        //    return await Connection.QuerySingleOrDefaultAsync<UserModel>(
        //        sql: "[CAP].[Get_Terms_Of_Use]",
        //        new { UserId = userId, ApplicationId = applicationId },
        //        transaction: Transaction,
        //        commandType: CommandType.StoredProcedure);
        //}

        //public async Task<bool> AddNewWCNAsync(string email, WCNModel model)
        //{
        //    var parameters = new DynamicParameters();
        //    parameters.Add("@Email", email);
        //    parameters.Add("@CompanyName", model.CompanyName);
        //    parameters.Add("@WCN", model.WCN);

        //    await Connection.ExecuteAsync(
        //        sql: "[CAP].[Add_New_WCN]",
        //        param: parameters,
        //        commandType: CommandType.StoredProcedure,
        //        transaction: Transaction);

        //    return true;
        //}

        //#endregion
    }
}


