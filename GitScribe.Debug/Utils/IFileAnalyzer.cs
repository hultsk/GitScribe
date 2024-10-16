using LibGit2Sharp;

namespace GitScribe.Debug.Utils;

/// <summary>
/// Defines methods for analyzing file changes and retrieving patch information in a Git repository.
/// </summary>
public interface IFileAnalyzer
{
   /// <summary>
   /// Retrieves the patches (differences) for a specified file in the Git repository.
   /// </summary>
   /// <param name="repositoryPath">The file system path to the Git repository.</param>
   /// <param name="entry">The status entry representing the file whose patches are being retrieved.</param>
   /// <returns>
   /// An enumerable collection of <see cref="Patch"/> objects that represent the differences for the specified file.
   /// </returns>
   IEnumerable<Patch> GetPatches(string repositoryPath, StatusEntry entry);

   /// <summary>
   /// Concatenates the content of the given patches into a single string.
   /// </summary>
   /// <param name="patches">An enumerable collection of <see cref="Patch"/> objects.</param>
   /// <returns>
   /// A string containing the concatenated content of the provided patches.
   /// </returns>
   string GetPatchContent(IEnumerable<Patch> patches);
}

