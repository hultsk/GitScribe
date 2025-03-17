using GitScribe.Core;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace GitScribe.Service
{
   public class GitScibreService : BackgroundService
   {
      private readonly ILogger<GitScibreService> m_logger;
      private readonly ICommitAssistant m_commitAssistant;
      private readonly IRepositoryManager m_repositoryManager;

      public GitScibreService(ILogger<GitScibreService> logger, ICommitAssistant commitAssistant, IRepositoryManager repositoryManager)
      {
         m_logger = logger;
         m_commitAssistant = commitAssistant;
         m_repositoryManager = repositoryManager;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         m_logger.LogInformation("GitScribeService started at: {Time}", DateTimeOffset.Now);

         while (!stoppingToken.IsCancellationRequested)
         {
            try
            {
               // Get all active repository IDs
               var repoIds = m_repositoryManager.GetRepositoryIds();
               m_logger.LogInformation("Processing {Count} repositories", repoIds.Count());

               //foreach (var repoId in repoIds)
               //{
               //   try
               //   {
               //      ProcessRepository(repoId);
               //   }
               //   catch (Exception repoEx)
               //   {
               //      m_logger.LogError(repoEx, "Error processing repository {RepositoryId}", repoId);
               //   }
               //}
            }
            catch (Exception ex)
            {
               m_logger.LogError(ex, "Error occurred while executing GitScribe");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Adjust timing as needed
         }
      }

      private void ProcessRepository(string repositoryId)
      {
         var repositoryInfo = m_repositoryManager.GetRepositoryInformation(repositoryId);
         if (repositoryInfo == null)
         {
            m_logger.LogWarning("Could not access repository information for {RepositoryId}", repositoryId);
            return;
         }

         // Log general repository info
         m_logger.LogInformation(
             "[{RepositoryId}] Repository Information: Path={Path}, WorkingDirectory={WorkingDirectory}, IsHeadDetached={IsHeadDetached}",
             repositoryId,
             repositoryInfo.Path,
             repositoryInfo.WorkingDirectory,
             repositoryInfo.IsHeadDetached
         );

         using var repo = new Repository(repositoryInfo.Path);

         // Get the current branch
         var branch = repo.Head;
         if (!repositoryInfo.IsHeadDetached)
         {
            m_logger.LogInformation("[{RepositoryId}] Current branch: {Branch}", repositoryId, branch.FriendlyName);
         }
         else
         {
            m_logger.LogWarning("[{RepositoryId}] Repository is in a detached HEAD state", repositoryId);
         }

         // Get uncommitted changes
         var status = repo.RetrieveStatus();
         int uncommittedChanges = status.Untracked.Count() + status.Modified.Count() + status.Staged.Count();
         m_logger.LogInformation("[{RepositoryId}] Uncommitted changes: {UncommittedChanges}", repositoryId, uncommittedChanges);

         // Get remote URL (if available)
         var remote = repo.Network.Remotes.FirstOrDefault();
         if (remote != null)
         {
            m_logger.LogInformation("[{RepositoryId}] Remote URL: {RemoteUrl}", repositoryId, remote.Url);
         }
         else
         {
            m_logger.LogWarning("[{RepositoryId}] No remote configured for this repository", repositoryId);
         }

         // Log detailed changes if there are any uncommitted changes
         if (uncommittedChanges > 0)
         {
            LogDetailedChanges(repositoryId, status);
         }
      }

      private void LogDetailedChanges(string repositoryId, RepositoryStatus status)
      {
         // Log untracked files
         if (status.Untracked.Any())
         {
            m_logger.LogInformation("[{RepositoryId}] Untracked files: {Count}", repositoryId, status.Untracked.Count());
            foreach (var file in status.Untracked.Take(10)) // Limit to prevent huge logs
            {
               m_logger.LogDebug("[{RepositoryId}] Untracked: {File}", repositoryId, file.FilePath);
            }

            if (status.Untracked.Count() > 10)
            {
               m_logger.LogDebug("[{RepositoryId}] ... and {Count} more untracked files", repositoryId, status.Untracked.Count() - 10);
            }
         }

         // Log modified files
         if (status.Modified.Any())
         {
            m_logger.LogInformation("[{RepositoryId}] Modified files: {Count}", repositoryId, status.Modified.Count());
            foreach (var file in status.Modified.Take(10))
            {
               m_logger.LogDebug("[{RepositoryId}] Modified: {File}", repositoryId, file.FilePath);
            }

            if (status.Modified.Count() > 10)
            {
               m_logger.LogDebug("[{RepositoryId}] ... and {Count} more modified files", repositoryId, status.Modified.Count() - 10);
            }
         }

         // Log staged files
         if (status.Staged.Any())
         {
            m_logger.LogInformation("[{RepositoryId}] Staged files: {Count}", repositoryId, status.Staged.Count());
            foreach (var file in status.Staged.Take(10))
            {
               m_logger.LogDebug("[{RepositoryId}] Staged: {File}", repositoryId, file.FilePath);
            }

            if (status.Staged.Count() > 10)
            {
               m_logger.LogDebug("[{RepositoryId}] ... and {Count} more staged files", repositoryId, status.Staged.Count() - 10);
            }
         }
      }



      public override async Task StartAsync(CancellationToken cancellationToken)
      {
         m_logger.LogInformation("GitScibreService is starting...");
         await base.StartAsync(cancellationToken);
      }

      public override async Task StopAsync(CancellationToken cancellationToken)
      {
         m_logger.LogInformation("GitScibreService is stopping...");
         await base.StopAsync(cancellationToken);
      }
   }
}
