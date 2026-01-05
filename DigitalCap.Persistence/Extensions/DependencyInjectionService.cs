using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Infrastructure.Service;
using DigitalCap.Persistence.Repositories;
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
            services.AddScoped<IGradeRepository, IGradeRepository>();
            services.AddScoped<IGradingRepository, IGradingRepository>();
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

            // Services
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IGradingService, GradingService>();
            services.AddScoped<ISecurityService, SecurityService>();
            services.AddScoped<ISurveyReportService, SurveyReportService>();
            services.AddScoped<IVesselService, VesselService>();

            return services;
        }
    }
}
