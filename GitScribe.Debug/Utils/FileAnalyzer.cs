using LibGit2Sharp;
using System.Text;

namespace GitScribe.Debug.Utils;

public class FileAnalyzer : IFileAnalyzer
{
   public IEnumerable<Patch> GetPatches(string repositoryPath, StatusEntry entry)
   {
      string filePath = Path.Combine(repositoryPath, entry.FilePath);

      using var repo = new Repository(repositoryPath);
      yield return repo.Diff.Compare<Patch>(repo.Head.Tip.Tree, repo.Index.WriteToTree(), [filePath]);
   }

   public string GetPatchContent(IEnumerable<Patch> patches)
   {
      StringBuilder patchContent = new();
      foreach (var patch in patches)
         patchContent.AppendLine(patch.Content);

      return patchContent.ToString();
   }
}