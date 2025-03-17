using GitScribe.Core;

namespace GitScribe.Service
{
   public class Program
   {
      public static void Main(string[] args)
      {
         CreateHostBuilder(args).Build().Run();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .UseWindowsService()
              .ConfigureServices((hostContext, services) =>
              {
                 services.AddRepositoryManager(hostContext.Configuration);
                 services.AddSingleton<ICommitAssistant, CommitAssistant>();

                 services.AddHostedService<GitScibreService>();
              });
   }

   public static class RepositoryManagerConfiguration
   {
      public static IServiceCollection AddRepositoryManager(this IServiceCollection services, IConfiguration configuration)
      {
         var repositorySettings = new RepositorySettings();
         configuration.GetSection(RepositorySettings.SectionName).Bind(repositorySettings);

         services.AddSingleton(repositorySettings);
         services.AddSingleton<IRepositoryManager, RepositoryManager>();

         return services;
      }
   }
}