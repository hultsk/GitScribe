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
}