using GitScribe.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

         var settings = new RepositorySettings();
         settings.Repositories.Add(new RepositoryConfig
         {
            Id = "test-repo",
            Name = "Test Repository",
            Path = RepositoryPath,
            IsActive = true
         });
         using var loggerFactory = LoggerFactory.Create(builder =>
         {
            builder.SetMinimumLevel(LogLevel.Information);
         });

         var logger = loggerFactory.CreateLogger<RepositoryManager>();

         IRepositoryManager repositoryManager = new RepositoryManager(settings, logger);
         ICommitAssistant commitAssistant = new CommitAssistant(repositoryManager, new(endpoint, apiKey, "gitscribe", "gpt-4o"));

         var patchContent = commitAssistant.CollectPatchContent();

         if (!string.IsNullOrEmpty(patchContent))
         {
            var (title, description) = await commitAssistant.GenerateCommitMessageAsync(patchContent);
            Console.WriteLine($"Suggested commit title: {title}");
            Console.WriteLine($"Suggested commit description: {description}");

            Console.WriteLine("Do you want to commit these changes? (Y/N)");
            var response = Console.ReadLine();

            if (response.Equals("Y", StringComparison.OrdinalIgnoreCase))
               repositoryManager.CommitChanges(title, description);
            else
               Console.WriteLine("No changes were committed.");
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