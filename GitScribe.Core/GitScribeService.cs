using LibGit2Sharp;
using Microsoft.SemanticKernel;
using System.Text;

namespace GitScribe.Core
{
   public class GitScribeService : IGitScribeService
   {
      private readonly Kernel m_kernel;
      private readonly IRepositoryManager m_repositoryManager;

      public GitScribeService(IRepositoryManager repositoryManager, string endpoint, string apiKey, string deploymentName, string modelId)
      {
         if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty.", nameof(endpoint));
         if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));
         if (string.IsNullOrWhiteSpace(deploymentName))
            throw new ArgumentException("Deployment name cannot be null or empty.", nameof(deploymentName));
         if (string.IsNullOrWhiteSpace(modelId))
            throw new ArgumentException("Model ID cannot be null or empty.", nameof(modelId));

         m_kernel = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey, null, modelId, new HttpClient()).Build();
         m_repositoryManager = repositoryManager ?? throw new ArgumentNullException(nameof(repositoryManager));
      }

      public string CollectPatchContent()
      {
         var fileStatusEntries = m_repositoryManager.RetrieveStatus();
         var combinedPatchContent = new StringBuilder();

         foreach (var entry in fileStatusEntries)
         {
            if (entry.State.HasFlag(FileStatus.ModifiedInIndex) ||
                entry.State.HasFlag(FileStatus.ModifiedInIndex) && entry.State.HasFlag(FileStatus.ModifiedInWorkdir) ||
                entry.State.HasFlag(FileStatus.RenamedInIndex) || 
                entry.State.HasFlag(FileStatus.TypeChangeInIndex) || 
                entry.State.HasFlag(FileStatus.NewInIndex) || 
                entry.State.HasFlag(FileStatus.DeletedFromIndex))
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

         var result = await m_kernel.InvokePromptAsync(GeneratePromptMessage(patchContent));
         var output = result.ToString().Trim();

         var splitOutput = output.Split(new[] { "Commit description:" }, StringSplitOptions.None);
         var title = splitOutput[0].Replace("Commit title:", "").Trim();
         var description = splitOutput.Length > 1 ? splitOutput[1].Trim() : string.Empty;

         return (title, description);
      }

      public string GeneratePromptMessage(string patchContent)
      {
return $@"
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
Commit description: [Your description here]";
      }
   }
}