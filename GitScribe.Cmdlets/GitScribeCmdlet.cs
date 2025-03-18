using GitScribe.Core;
using System.Management.Automation;

namespace GitScribe.Cmdlets
{
   [Cmdlet("Git", "Scribe")]
   [OutputType(typeof(string))]
   public class GitScribeCmdlet : PSCmdlet
   {
      [Parameter(Mandatory = true)]
      public required string RepositoryPath { get; set; }

      [Parameter(Mandatory = false)]
      public required string Endpoint { get; set; }

      [Parameter(Mandatory = false)]
      public required string ApiKey { get; set; }

      [Parameter(Mandatory = false)]
      public required string DeploymentName { get; set; }

      [Parameter(Mandatory = false)]
      public required string ModelId { get; set; }

      private IRepositoryManager? m_repositoryManager;
      private ICommitAssistant? m_commitAssistant;

      protected override void BeginProcessing()
      {
         base.BeginProcessing();

         try
         {
            var loggerFactory = new PowerShellLoggerFactory(this);
            var settings = new RepositorySettings();
            settings.Repositories.Add(new RepositoryConfig("Test Repository", RepositoryPath));

            m_repositoryManager = new RepositoryManager(settings, loggerFactory.CreateLogger<RepositoryManager>());
            m_commitAssistant = new CommitAssistant(m_repositoryManager, new(Endpoint, ApiKey, DeploymentName, ModelId));
         }
         catch (Exception ex)
         {
            WriteError(new ErrorRecord(ex, "GitScribeInitializationError", ErrorCategory.NotSpecified, null));
            throw new InvalidOperationException("Failed to initialize GitScribe services.", ex);
         }
      }

      protected override void ProcessRecord()
      {
         base.ProcessRecord();

         var patchContent = m_commitAssistant!.CollectPatchContent();

         if (string.IsNullOrWhiteSpace(patchContent))
         {
            WriteWarning("No relevant changes detected.");
            return;
         }

         var (title, description) = GenerateCommitMessage(patchContent);
         PresentCommitDetails(title, description);

         string action;
         do
         {
            ConsoleKeyInfo keyInfo = GetUserInputForActionKey();
            Console.Clear();

            action = keyInfo.Key switch
            {
               ConsoleKey.R => "R",
               ConsoleKey.E => "E",
               ConsoleKey.C or ConsoleKey.Enter => "C",
               ConsoleKey.D or ConsoleKey.Escape => "D",
               _ => ""
            };

            switch (action)
            {
               case "R":
                  (title, description) = RegenerateCommitMessage(patchContent);
                  PresentCommitDetails(title, description);
                  break;
               case "E":
                  (title, description) = EditCommitMessage(title, description);
                  PresentCommitDetails(title, description);
                  break;
               case "C":
                  CommitChanges(title, description);
                  break;
               case "D":
                  DiscardChanges();
                  break;
               default:
                  WriteWarning("Invalid input. " + GetActions());
                  break;
            }
         } while (action != "C" && action != "D");
      }

      private ConsoleKeyInfo GetUserInputForActionKey()
      {
         Console.WriteLine("Choose an action: " + GetActions());
         return Console.ReadKey(intercept: true);
      }

      private string GetActions()
      {
         return "(R)egenerate, (E)dit, (C)ommit[Enter], (D)iscard[Escape]";
      }

      private (string title, string description) GenerateCommitMessage(string patchContent)
      {
         var commitMessage = m_commitAssistant!.GenerateCommitMessageAsync(patchContent).Result;
         return (commitMessage.Title, commitMessage.Description);
      }

      private void PresentCommitDetails(string title, string description)
      {
         WriteObject("---- Commit Preview ----");
         WriteObject($"Title:\n {title}");
         WriteObject($"Description:\n {description}");
         WriteObject("------------------------");
      }

      private (string title, string description) RegenerateCommitMessage(string patchContent)
      {
         var newCommitMessage = m_commitAssistant!.GenerateCommitMessageAsync(patchContent).Result;
         return (newCommitMessage.Title, newCommitMessage.Description);
      }

      private (string title, string description) EditCommitMessage(string currentTitle, string currentDescription)
      {
         var newTitle = ReadLine($"Enter new commit title\n Current: {currentTitle}");
         var titleToUse = string.IsNullOrEmpty(newTitle) 
            ? currentTitle 
            : newTitle;

         var newDescription = ReadLine($"Enter new commit description\n Current: {currentDescription}");
         var descriptionToUse = string.IsNullOrEmpty(newDescription) 
            ? currentDescription 
            : newDescription;

         return (titleToUse, descriptionToUse);
      }

      private void CommitChanges(string title, string description)
      {
         m_repositoryManager!.CommitChanges(title, description);
         WriteObject("Changes committed successfully.");
      }

      private void DiscardChanges()
      {
         WriteObject("Changes discarded.");
      }

      private string ReadLine(string prompt)
      {
         WriteObject(prompt);
         return Console.ReadLine()!;
      }
   }
}