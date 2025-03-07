using GitScribe.Core;
using Microsoft.AspNetCore.Builder;

namespace GitScribe.Service
{
   public class GitScribeService : BackgroundService
   {
      private readonly ILogger<GitScribeService> m_logger;
      private readonly IHostApplicationLifetime m_applicationLifetime;
      private WebApplication? m_webApp;

      public GitScribeService(ILogger<GitScribeService> logger, IHostApplicationLifetime applicationLifetime)
      {
         m_logger = logger;
         m_applicationLifetime = applicationLifetime;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         try
         {
            m_webApp = BuildWebApp();
            await m_webApp.StartAsync(stoppingToken);

            m_logger.LogInformation("gRPC service is running in the background.");

            await Task.Delay(Timeout.Infinite, stoppingToken);
         }
         catch (Exception ex)
         {
            m_logger.LogError(ex, "An error occurred while starting the gRPC service.");
            throw;
         }
      }

      private WebApplication BuildWebApp()
      {
         var builder = WebApplication.CreateBuilder();

         builder.Services.AddGrpc();
         builder.Services.AddSingleton<IRepositoryManager>(provider =>
         {
            var config = provider.GetRequiredService<IConfiguration>()
                                 .GetSection(RepositorySettings.SectionName)
                                 .Get<RepositorySettings>()
                                 ?? throw new InvalidOperationException("RepositorySettings configuration section is missing or invalid.");

            return new RepositoryManager(config);
         });
         builder.Services.AddSingleton<IGitScribeService>(provider =>
         {
            var config = provider.GetRequiredService<IConfiguration>()
                                 .GetSection(GitScribeSettings.SectionName)
                                 .Get<GitScribeSettings>()
                                 ?? throw new InvalidOperationException("GitScribeSettings configuration section is missing or invalid.");

            return new Core.GitScribeService(provider.GetRequiredService<IRepositoryManager>(), config);
         });

         var app = builder.Build();

         app.MapGrpcService<GitScribeService>();

         return app;
      }

      public override Task StopAsync(CancellationToken cancellationToken)
      {
         if (m_webApp != null)
         {
            m_logger.LogInformation("Stopping gRPC service.");
            return m_webApp.StopAsync(cancellationToken);
         }

         return Task.CompletedTask;
      }
   }
}