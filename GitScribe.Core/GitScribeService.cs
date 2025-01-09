using LibGit2Sharp;
using Microsoft.SemanticKernel;
using System.Text;

namespace GitScribe.Core
{
   public class GitScribeService : IGitScribeService
   {
      private readonly Kernel m_kernel;
      private readonly IRepositoryManager m_repositoryManager;

      private const string DeploymentName = "gitscribe";
      private const string ModelId = "gpt-4o";

      public GitScribeService(IRepositoryManager repositoryManager, string endpoint, string apiKey)
      {
         if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty.", nameof(endpoint));
         if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));

         m_kernel = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(DeploymentName, endpoint, apiKey, null, ModelId, new HttpClient()).Build();
         m_repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
      }

      public string CollectPatchContent()
      {
         var fileStatusEntries = m_repositoryManager.RetrieveStatus();
         var combinedPatchContent = new StringBuilder();

         foreach (var entry in fileStatusEntries)
         {
            if (entry.State == FileStatus.ModifiedInIndex || entry.State == FileStatus.NewInIndex || entry.State == FileStatus.DeletedFromIndex)
            {
               var patches = m_repositoryManager.GetPatches(entry);
               var patchContent = m_repositoryManager.GetPatchContent(patches);
               combinedPatchContent.AppendLine(patchContent);
            }
         }

         return combinedPatchContent.ToString();
      }

      public async Task<(string Title, string Description)> GenerateCommitMessageAsync(string patchContent)
      {
         if (string.IsNullOrWhiteSpace(patchContent))
            throw new ArgumentException("Patch content cannot be null or empty.", nameof(patchContent));

         var promptTemplate = $@"
            Please generate a concise Git commit message based on the following changes:

            Changes:
            {patchContent}

            The output should include:
            - A commit title:
               - Written in imperative mood
               - Clear and concise
               - Ideally 50 characters or less

            - A commit description:
               - Written in imperative mood
               - Provides a clear summary of the changes
               - Includes any additional context that might be helpful

            Format your response as follows:
            Commit title: [Your title here]
            Commit description: [Your description here]
        ";

         var result = await m_kernel.InvokePromptAsync(promptTemplate);
         var output = result.ToString().Trim();

         var splitOutput = output.Split(new[] { "Commit description:" }, StringSplitOptions.None);
         var title = splitOutput[0].Replace("Commit title:", "").Trim();
         var description = splitOutput.Length > 1 ? splitOutput[1].Trim() : string.Empty;

         return (title, description);
      }
   }
}