using LibGit2Sharp;

namespace GitScribe.Debug.Utils;

/// <summary>
/// Defines methods for managing and interacting with Git repositories.
/// </summary>
public interface IRepositoryManager
{
   /// <summary>
   /// Retrieves the status of the files in the specified Git repository.
   /// </summary>
   /// <param name="repositoryPath">The file system path to the Git repository.</param>
   /// <returns>
   /// An enumerable collection of <see cref="StatusEntry"/> objects representing the status of files in the repository.
   /// </returns>
   IEnumerable<StatusEntry> RetrieveStatus(string repositoryPath);
}
