using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Users;
using OrchardCore.Users.Services;
using Repro.WebJobs.Extensions;

namespace Repro.WebJobs.Jobs
{
    [Disable("DisabledWebjobs:TestJob")]
    public class TestJob
    {
        private readonly IServiceProvider _serviceProvider;

        public TestJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Singleton]
#if DEBUG
        public async Task TestTaskDebug([TimerTrigger("*/10 * * * * *", RunOnStartup = true)] TimerInfo timerInfo, TextWriter log, CancellationToken cancellationToken)
#else
        public async Task TestTask([TimerTrigger("0 0 5 * * *", RunOnStartup = true)] TimerInfo timerInfo, TextWriter log, CancellationToken cancellationToken)
#endif
        {
            if (timerInfo.IsPastDue)
            {
                return;
            }

            await log.WriteLineAsync($"{this.GetType().Name} started at {DateTime.UtcNow}.");

            var executed = await _serviceProvider.TryExecuteAtTenantScopeAsync("Default", async serviceProvider =>
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<IUser>>();
                var userService = serviceProvider.GetRequiredService<IUserService>();
                var contentManager = serviceProvider.GetRequiredService<IContentManager>();

                //user manager
                var dawid = await userManager.FindByEmailAsync("dawid@example.com");
                var admin = await userManager.FindByNameAsync("admin");

                //user service
                var adminUser = await userService.GetUserAsync("admin");

                //contentManager
                var contentItem = await contentManager.GetAsync("not existing guid");
            });

            await log.WriteLineAsync($"{this.GetType().Name} finished at {DateTime.UtcNow}.");
        }

        public record Test(string ContentItemId);

        [Singleton]
        public async Task TestQueue([QueueTrigger("test")] Test model, TextWriter log, CancellationToken cancellationToken)
        {
            await log.WriteLineAsync($"{this.GetType().Name} started at {DateTime.UtcNow}.");

            var executed = await _serviceProvider.TryExecuteAtDefaultTenantScopeAsync(async serviceProvider => {
                var contentManager = serviceProvider.GetRequiredService<IContentManager>();
                var contentItem = await contentManager.GetAsync(model.ContentItemId);
            });

            await log.WriteLineAsync($"{this.GetType().Name} finished at {DateTime.UtcNow}.");
        }

    }
}

