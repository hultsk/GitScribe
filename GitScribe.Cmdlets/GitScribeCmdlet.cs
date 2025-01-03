using GitScribe.Core;
using System.Management.Automation;

namespace GitScribe.Cmdlets
{
   [Cmdlet("Git", "Scribe")]
   [OutputType(typeof(string))]
   public class GitScribeCmdlet : Cmdlet
   {
      [Parameter(Mandatory = true)]
      public required string RepositoryPath { get; set; }

      [Parameter(Mandatory = true)]
      public required string Endpoint { get; set; }

      [Parameter(Mandatory = true)]
      public required string ApiKey { get; set; }

      private IRepositoryManager? m_repositoryManager;
      private IGitScribeService? m_gitScribe;

      protected override void BeginProcessing()
      {
         base.BeginProcessing();

         try
         {
            m_repositoryManager = new RepositoryManager(RepositoryPath);
            m_gitScribe = new GitScribeService(m_repositoryManager, Endpoint, ApiKey);
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

         var patchContent = m_gitScribe!.CollectPatchContent();

         if (string.IsNullOrWhiteSpace(patchContent))
         {
            WriteWarning("No relevant changes detected.");
            return;
         }

         // Generate commit message asynchronously
         var (title, description) = GenerateCommitMessage(patchContent);

         // Present commit details and ask for user input
         PresentCommitDetails(title, description);

         string action;
         do
         {
            action = GetUserInputForAction();

            // Clear console before each new action
            Console.Clear();

            switch (action.ToUpper())
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
                  WriteWarning("Invalid input. Please choose a valid option (R, E, C, D).");
                  break;
            }
         } while (action.ToUpper() != "C" && action.ToUpper() != "D");
      }

      private (string title, string description) GenerateCommitMessage(string patchContent)
      {
         var commitMessage = m_gitScribe!.GenerateCommitMessageAsync(patchContent).Result;
         return (commitMessage.Title, commitMessage.Description);
      }

      private void PresentCommitDetails(string title, string description)
      {
         WriteObject("---- Commit Preview ----");
         WriteObject($"Title: {title}");
         WriteObject($"Description:\n{description}");
         WriteObject("------------------------");
      }

      private string GetUserInputForAction()
      {
         WriteObject("(R to regenerate, E to edit, C to commit, D to discard)");
         return Console.ReadLine()!;
      }

      private (string title, string description) RegenerateCommitMessage(string patchContent)
      {
         var newCommitMessage = m_gitScribe!.GenerateCommitMessageAsync(patchContent).Result;
         return (newCommitMessage.Title, newCommitMessage.Description);
      }

      private (string title, string description) EditCommitMessage(string currentTitle, string currentDescription)
      {
         var newTitle = ReadLine($"Enter new commit title (current: {currentTitle}):");
         var titleToUse = string.IsNullOrWhiteSpace(newTitle) 
            ? currentTitle 
            : newTitle;

         var newDescription = ReadLine($"Enter new commit description (current: {currentDescription}):");
         var descriptionToUse = string.IsNullOrWhiteSpace(newDescription) 
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