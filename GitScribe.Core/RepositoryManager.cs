using LibGit2Sharp;
using System.Text;

namespace GitScribe.Utils;

public class RepositoryManager : IRepositoryManager
{
   private readonly string m_repositoryPath;

   public RepositoryManager(string repositoryPath)
   {
      m_repositoryPath = repositoryPath;
   }

   public IEnumerable<Patch> GetPatches(StatusEntry entry)
   {
      string filePath = Path.Combine(m_repositoryPath, entry.FilePath);

      using var repo = new Repository(m_repositoryPath);
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
      using (var repo = new Repository(m_repositoryPath))
      {
         return repo.RetrieveStatus();
      }
   }

   public void CommitChanges(string commitTitle, string commitDescription)
   {
      if (string.IsNullOrWhiteSpace(commitTitle))
         throw new ArgumentException("Commit title cannot be null or empty.", nameof(commitTitle));

      if (string.IsNullOrWhiteSpace(commitDescription))
         throw new ArgumentException("Commit description cannot be null or empty.", nameof(commitDescription));

      using (var repo = new Repository(m_repositoryPath))
      {
         // Check if the repository is dirty (has uncommitted changes)
         if (repo.RetrieveStatus().Any())
         {
            var signature = repo.Config.BuildSignature(DateTimeOffset.Now);

            // Combine the title and description
            var fullCommitMessage = $"{commitTitle}\n\n{commitDescription}";

            // Create the commit with title and description
            var commit = repo.Commit(fullCommitMessage, signature, signature);
         }
         else
         {
            throw new InvalidOperationException("There are no changes to commit.");
         }
      }
   }

}