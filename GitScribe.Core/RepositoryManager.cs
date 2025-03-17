using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GitScribe.Core;

public class RepositoryManager : IRepositoryManager
{
   private readonly RepositorySettings m_settings;
   private readonly ILogger<RepositoryManager> m_logger;
   private readonly RepositoryConfig m_currentRepositoryConfig;

   public RepositoryManager(RepositorySettings settings, ILogger<RepositoryManager> logger)
   {
      m_settings = settings;
      m_logger = logger;
      m_currentRepositoryConfig = m_settings.Repositories.FirstOrDefault() ?? throw new ArgumentNullException(nameof(settings.Repositories));
   }

   public IEnumerable<string> GetRepositoryIds()
   {
      return m_settings.Repositories
          .Where(r => r.IsActive)
          .Select(r => r.Id);
   }

   public RepositoryInformation? GetRepositoryInformation(string repositoryId)
   {
      var repoConfig = m_settings.Repositories.FirstOrDefault(r => r.Id == repositoryId && r.IsActive);

      if (repoConfig == null)
      {
         m_logger.LogWarning("Repository with ID {RepositoryId} not found or inactive", repositoryId);
         return null;
      }

      try
      {
         using (var repo = new Repository(repoConfig.Path))
         {
            return repo.Info;
         }
      }
      catch (Exception ex)
      {
         m_logger.LogError(ex, "Error accessing repository {RepositoryId} at {Path}", repositoryId, repoConfig.Path);
         return null;
      }
   }

   public IEnumerable<(string Id, RepositoryInformation Info)> GetAllRepositoryInformation()
   {
      var results = new List<(string Id, RepositoryInformation Info)>();

      foreach (var repo in m_settings.Repositories.Where(r => r.IsActive))
      {
         try
         {
            using (var gitRepo = new Repository(repo.Path))
            {
               results.Add((repo.Id, gitRepo.Info));
            }
         }
         catch (Exception ex)
         {
            m_logger.LogError(ex, "Error accessing repository {RepositoryId} at {Path}", repo.Id, repo.Path);
         }
      }

      return results;
   }

   public bool AddRepository(RepositoryConfig config)
   {
      if (m_settings.Repositories.Any(r => r.Id == config.Id))
      {
         m_logger.LogWarning("Repository with ID {RepositoryId} already exists", config.Id);
         return false;
      }

      m_settings.Repositories.Add(config);
      return true;
   }

   public bool RemoveRepository(string repositoryId)
   {
      var repo = m_settings.Repositories.FirstOrDefault(r => r.Id == repositoryId);
      if (repo == null)
      {
         return false;
      }

      m_settings.Repositories.Remove(repo);
      return true;
   }

   public bool UpdateRepository(RepositoryConfig config)
   {
      var existingRepo = m_settings.Repositories.FirstOrDefault(r => r.Id == config.Id);
      if (existingRepo == null)
      {
         return false;
      }

      // Remove old config and add updated one
      m_settings.Repositories.Remove(existingRepo);
      m_settings.Repositories.Add(config);
      return true;
   }

   public IEnumerable<Patch> GetPatches(StatusEntry entry)
   {
      string filePath = Path.Combine(m_currentRepositoryConfig.Path, entry.FilePath);

      using var repo = new Repository(m_currentRepositoryConfig.Path);
      yield return repo.Diff.Compare<Patch>(repo.Head.Tip.Tree, repo.Index.WriteToTree(), [filePath]);
   }

   public string GetPatchContent(IEnumerable<Patch> patches)
   {
      StringBuilder patchContent = new();
      foreach (var patch in patches)
         patchContent.AppendLine(patch.Content);

      return patchContent.ToString();
   }

   public IEnumerable<StatusEntry> RetrieveStatus()
   {
      using (var repo = new Repository(m_currentRepositoryConfig.Path))
      {
         return repo.RetrieveStatus();
      }
   }

   public void CommitChanges(string commitTitle, string commitDescription)
   {
      if (string.IsNullOrWhiteSpace(commitTitle))
         throw new ArgumentException("Commit title cannot be null or empty.", nameof(commitTitle));

      if (string.IsNullOrEmpty(commitDescription))
         throw new ArgumentException("Commit description cannot be null or empty.", nameof(commitDescription));

      using (var repo = new Repository(m_currentRepositoryConfig.Path))
      {
         if (repo.RetrieveStatus().Any())
         {
            var signature = repo.Config.BuildSignature(DateTimeOffset.Now);
            var fullCommitMessage = $"{commitTitle}\n\n{commitDescription}";
            var commit = repo.Commit(fullCommitMessage, signature, signature);
         }
         else
         {
            throw new InvalidOperationException("There are no changes to commit.");
         }
      }
   }
}