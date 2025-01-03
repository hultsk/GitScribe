using GitScribe.Core;
using GitScribe.Utils;
using Microsoft.Extensions.Configuration;

namespace GitScribe.Debug
{
   internal class Program
   {
      private const string RepositoryPath = @"F:\hultsk\GitScribe";

      private static async Task Main(string[] args)
      {
         IConfiguration configuration = SetupConfiguration();

         string endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is not configured.");
         string apiKey = configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("AzureOpenAI:ApiKey is not configured.");

         IRepositoryManager repositoryManager = new RepositoryManager(RepositoryPath);
         IGitScribeService gitScribe = new GitScribeService(repositoryManager, endpoint, apiKey);

         var patchContent = gitScribe.CollectPatchContent();

         if (!string.IsNullOrEmpty(patchContent))
         {
            var (title, description) = await gitScribe.GenerateCommitMessageAsync(patchContent);
            Console.WriteLine($"Suggested commit title: {title}");
            Console.WriteLine($"Suggested commit description: {description}");
         }
         else
         {
            Console.WriteLine("No relevant changes detected.");
         }
      }

      private static IConfiguration SetupConfiguration()
      {
         var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
         return builder.Build();
      }
   }
}