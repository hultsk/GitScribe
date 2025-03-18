using LibGit2Sharp;

namespace GitScribe.Core;

/// <summary>
/// Defines methods for managing and interacting with Git repositories.
/// </summary>
public interface IRepositoryManager
{
   IEnumerable<string> GetRepositories();
   RepositoryInformation? GetRepositoryInformation(string name);
   IEnumerable<(string Name, RepositoryInformation Info)> GetAllRepositoryInformation();
   bool AddRepository(RepositoryConfig config);
   bool RemoveRepository(string name);
   bool UpdateRepository(RepositoryConfig config);

   /// <summary>
   /// Retrieves the patches (differences) for a specified file in the Git repository.
   /// </summary>
   /// <param name="entry">The status entry representing the file whose patches are being retrieved.</param>
   /// <returns>
   /// An enumerable collection of <see cref="Patch"/> objects that represent the differences for the specified file.
   /// </returns>
   IEnumerable<Patch> GetPatches(StatusEntry entry);

   /// <summary>
   /// Concatenates the content of the given patches into a single string.
   /// </summary>
   /// <param name="patches">An enumerable collection of <see cref="Patch"/> objects.</param>
   /// <returns>
   /// A string containing the concatenated content of the provided patches.
   /// </returns>
   string GetPatchContent(IEnumerable<Patch> patches);

   /// <summary>
   /// Commits the changes in the repository with a specified title and description for the commit message.
   /// </summary>
   /// <param name="commitTitle">The title of the commit message. This should be a concise summary, ideally 50 characters or less.</param>
   /// <param name="commitDescription">The description of the commit message, providing additional context or details about the changes.</param>
   /// <exception cref="ArgumentException">
   /// Thrown when either <paramref name="commitTitle"/> or <paramref name="commitDescription"/> is null or empty.
   /// </exception>
   /// <exception cref="InvalidOperationException">
   /// Thrown when there are no changes to commit in the repository.
   /// </exception>
   void CommitChanges(string commitTitle, string commitDescription);

   /// <summary>
   /// Retrieves the status of the files in the specified Git repository.
   /// </summary>
   /// <returns>
   /// An enumerable collection of <see cref="StatusEntry"/> objects representing the status of files in the repository.
   /// </returns>
   IEnumerable<StatusEntry> RetrieveStatus();
}
