using LibGit2Sharp;

namespace GitScribe.Utils;

public class RepositoryManager : IRepositoryManager
{
   public IEnumerable<StatusEntry> RetrieveStatus(string repositoryPath)
   {
      using (var repo = new Repository(repositoryPath))
      {
         return repo.RetrieveStatus();
      }
   }
}