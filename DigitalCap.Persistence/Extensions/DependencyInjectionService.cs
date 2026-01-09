using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Infrastructure.Service;
using DigitalCap.Persistence.Repositories;
using DigitalCAP.Core.Services;
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
            services.AddScoped<IUserAccountRepository, UserAccountRepository>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<IPlatformUserRepository, PlatformUserRepository>();

            //services.AddScoped<ISecurityClientRepository, SecurityClientRepository>();

            // Services
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IUserAccountService, UserAccountService>();
            //services.AddScoped<ISecurityService, SecurityService>();
            services.AddScoped<ISecurityClientService, SecurityClientService>();
            services.AddScoped<IPlatformUserService, PlatformUserService>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
