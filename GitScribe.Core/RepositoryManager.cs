using LibGit2Sharp;
using System.Text;

namespace GitScribe.Core;

public class RepositoryManager : IRepositoryManager
{
   private readonly RepositorySettings m_repositorySettings;

   public RepositoryManager(RepositorySettings repositorySettings)
   {
      m_repositorySettings = repositorySettings;
   }

   public IEnumerable<Patch> GetPatches(StatusEntry entry)
   {
      string filePath = Path.Combine(m_repositorySettings.RepositoryPath, entry.FilePath);

      using var repo = new Repository(m_repositorySettings.RepositoryPath);
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
      using (var repo = new Repository(m_repositorySettings.RepositoryPath))
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

      using (var repo = new Repository(m_repositorySettings.RepositoryPath))
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