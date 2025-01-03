namespace GitScribe.Core
{
   /// <summary>
   /// Interface for GitScribe service to analyze repositories and generate commit messages.
   /// </summary>
   public interface IGitScribeService
   {
      /// <summary>
      /// Analyzes the repository and collects patch content for relevant files.
      /// </summary>
      /// <returns>The combined patch content of relevant files.</returns>
      string CollectPatchContent();

      /// <summary>
      /// Generates a commit message based on provided patch content.
      /// </summary>
      /// <param name="patchContent">The patch content.</param>
      /// <returns>A tuple containing the commit title and description.</returns>
      Task<(string Title, string Description)> GenerateCommitMessageAsync(string patchContent);
   }
}
