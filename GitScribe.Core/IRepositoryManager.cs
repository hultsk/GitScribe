﻿using LibGit2Sharp;

namespace GitScribe.Utils;

/// <summary>
/// Defines methods for managing and interacting with Git repositories.
/// </summary>
public interface IRepositoryManager
{
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

   void CommitChanges(string commitTitle, string commitDescription);

   /// <summary>
   /// Retrieves the status of the files in the specified Git repository.
   /// </summary>
   /// <returns>
   /// An enumerable collection of <see cref="StatusEntry"/> objects representing the status of files in the repository.
   /// </returns>
   IEnumerable<StatusEntry> RetrieveStatus();
}
