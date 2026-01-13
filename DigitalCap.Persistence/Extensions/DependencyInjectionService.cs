using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Infrastructure.Service;
using DigitalCap.Persistence.Repositories;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Persistence.Extensions
{
    public static class DependencyInjectionService
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {

            // DbContext
            //services.AddDbContext<AppDbContext>(options =>
            //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            //);

            // UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositories
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IBlobStorageRepository, BlobStorageRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IDescriptionRepository, DescriptionRepository>();
            services.AddScoped<IFileStorageRepository, FileStorageRepository>();
            services.AddScoped<IFreedomAPIRepository, FreedomAPIRepository>();
            services.AddScoped<IGradeRepository, GradeRepository>();
            services.AddScoped<IGradingRepository, GradingRepository>();
            services.AddScoped<IPlatformUserRepository, PlatformUserRepository>();
            services.AddScoped<IReportPartRepository, ReportPartRepository>();
            services.AddScoped<ISurveyReportRepository, SurveyReportRepository>();
            services.AddScoped<ISyncRepository, SyncRepository>();
            services.AddScoped<ITankRepository, TankRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITransferDataOnlinetoOfflineRepository, TransferDataOnlinetoOfflineRepository>();
            services.AddScoped<IUserAccountRepository, UserAccountRepository>();
            services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IVesselRepository, VesselRepository>();
            services.AddScoped<IProjectReportRepository, ProjectReportRepository>();
            services.AddScoped<IUserAccountRepository, UserAccountRepository>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<IPlatformUserRepository, PlatformUserRepository>();
            services.AddScoped<ISecurityClientRepository, SecurityClientRepository>();


            //services.AddScoped<ISecurityClientRepository, SecurityClientRepository>();

            services.AddHttpClient<IHttpService, HttpService>();
            // Services
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IGradingService, GradingService>();
            services.AddScoped<ISecurityService, SecurityService>();
            services.AddScoped<ISurveyReportService, SurveyReportService>();
            services.AddScoped<IVesselService, VesselService>();

            //services.AddTransient(serviceProvider =>
            //{
            //    // Note: Using a lazy here so we no longer require the service provider instance after
            //    //       the first usage of this Func result. If we allow late usage of the service provider,
            //    //       it is more likely to be disposed and will throw an exception upon usage.
            //    var lazyResult = new Lazy<ApplicationUser>(serviceProvider.GetService<ApplicationUser>);
            //    return new Func<ApplicationUser>(() => lazyResult.Value);
            //});

            services.AddScoped<IUserAccountService, UserAccountService>();
            //services.AddScoped<ISecurityService, SecurityService>();
            services.AddScoped<ISecurityClientService, SecurityClientService>();
            services.AddScoped<IPlatformUserService, PlatformUserService>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
