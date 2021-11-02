using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using System;
using System.Threading.Tasks;

namespace Repro.WebJobs.Extensions
{
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Trying to execute delagete using shell scope of provided tenant, only when exists and it's running
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="tenant"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        public async static Task<bool> TryExecuteAtTenantScopeAsync(this IServiceProvider serviceProvider, string tenant, Func<IServiceProvider, Task> execute)
        {
            if (!string.IsNullOrEmpty(tenant))
            {
                var shellHost = serviceProvider.GetRequiredService<IShellHost>();
                await shellHost.InitializeAsync();
                if (shellHost.TryGetSettings(tenant, out var settings))
                {
                    if (settings.State == TenantState.Running)
                    {
                        var shellScope = await shellHost.GetScopeAsync(settings);
                        try
                        {
                            await shellScope.UsingAsync(async scope =>
                            {
                                await execute(scope.ServiceProvider);
                            });
                            return true;
                        }
                        catch
                        {
                            //catch everything
                        }
                    }
                }
            }
            return false;
        }

        public async static Task<bool> TryExecuteAtDefaultTenantScopeAsync(this IServiceProvider serviceProvider, Func<IServiceProvider, Task> execute)
        {
            return await serviceProvider.TryExecuteAtTenantScopeAsync("Default", execute);
        }

        /// <summary>
        /// Execute delagete using shell scope of provided tenant, only when exists and it's running
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="tenant"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        public async static Task ExecuteAtTenantScopeAsync(this IServiceProvider serviceProvider, string tenant, Func<IServiceProvider, Task> execute)
        {
            if (string.IsNullOrEmpty(tenant))
            {
                throw new ArgumentException("Invalid tenant name.");
            }
            var shellHost = serviceProvider.GetRequiredService<IShellHost>();
            await shellHost.InitializeAsync();
            if (shellHost.TryGetSettings(tenant, out var settings))
            {
                if (settings.State == TenantState.Running)
                {
                    var shellScope = await shellHost.GetScopeAsync(settings);
                    await shellScope.UsingAsync(async scope =>
                    {
                        await execute(scope.ServiceProvider);
                    });
                }
                else
                {
                    throw new InvalidOperationException("Tenant is not running.");
                }
            }
            else
            {
                throw new ArgumentException($"Tenant with name '{tenant}' doesn't exist.");
            }
        }

        public async static Task ExecuteAtDefaultTenantScopeAsync(this IServiceProvider serviceProvider, Func<IServiceProvider, Task> execute)
        {
            await serviceProvider.ExecuteAtTenantScopeAsync("Default", execute);
        }
    }
}
