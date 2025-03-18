using GitScribe.Core;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace GitScribe.Service
{
   public class GitScibreService : BackgroundService
   {
      private readonly ILogger<GitScibreService> m_logger;
      private readonly IRepositoryManager m_repositoryManager;

      public GitScibreService(ILogger<GitScibreService> logger, IRepositoryManager repositoryManager)
      {
         m_logger = logger;
         m_repositoryManager = repositoryManager;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         m_logger.LogInformation("GitScribe started at: {Time}", DateTimeOffset.Now);

         while (!stoppingToken.IsCancellationRequested)
         {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
         }
      }

      public RepositoryInformation GetRepositoryInformation(string name) => m_repositoryManager.GetRepositoryInformation(name);
      public RepositoryStatus GetRepositoryStatus(string path) => m_repositoryManager.GetRepositoryStatus(path);

      public IEnumerable<string> GetRepositories() => m_repositoryManager.GetRepositories();

      public override async Task StartAsync(CancellationToken cancellationToken)
      {
         m_logger.LogInformation("GitScibre is starting...");
         await base.StartAsync(cancellationToken);
      }

      public override async Task StopAsync(CancellationToken cancellationToken)
      {
         m_logger.LogInformation("GitScibre is stopping...");
         await base.StopAsync(cancellationToken);
      }



      private void LogDetailedChanges(string name, RepositoryStatus status)
      {
         // Log untracked files
         if (status.Untracked.Any())
         {
            m_logger.LogInformation("[{Name}] Untracked files: {Count}", name, status.Untracked.Count());
            foreach (var file in status.Untracked.Take(10)) // Limit to prevent huge logs
            {
               m_logger.LogDebug("[{Name}] Untracked: {File}", name, file.FilePath);
            }

            if (status.Untracked.Count() > 10)
            {
               m_logger.LogDebug("[{Name}] ... and {Count} more untracked files", name, status.Untracked.Count() - 10);
            }
         }

         // Log modified files
         if (status.Modified.Any())
         {
            m_logger.LogInformation("[{Name}] Modified files: {Count}", name, status.Modified.Count());
            foreach (var file in status.Modified.Take(10))
            {
               m_logger.LogDebug("[{Name}] Modified: {File}", name, file.FilePath);
            }

            if (status.Modified.Count() > 10)
            {
               m_logger.LogDebug("[{Name}] ... and {Count} more modified files", name, status.Modified.Count() - 10);
            }
         }

         // Log staged files
         if (status.Staged.Any())
         {
            m_logger.LogInformation("[{Name}] Staged files: {Count}", name, status.Staged.Count());
            foreach (var file in status.Staged.Take(10))
            {
               m_logger.LogDebug("[{Name}] Staged: {File}", name, file.FilePath);
            }

            if (status.Staged.Count() > 10)
            {
               m_logger.LogDebug("[{Name}] ... and {Count} more staged files", name, status.Staged.Count() - 10);
            }
         }
      }
   }
}