using LibGit2Sharp;
using Moq;
using System.Text;

namespace GitScribe.Core.Tests;

[TestClass]
public class GitScribeCommitAssistantTests
{
   private Mock<IRepositoryManager> m_repositoryManagerMock = default!;

   [TestInitialize]
   public void Setup()
   {
      m_repositoryManagerMock = new Mock<IRepositoryManager>();
   }

   [TestMethod]
   [ExpectedException(typeof(ArgumentException))]
   public void Constructor_ShouldThrowException_WhenEndpointIsNullOrWhiteSpace()
   {
      // Act
      new GitScribeCommitAssistant(m_repositoryManagerMock.Object, new(string.Empty, "apiKey", "deploymentName", "modelId"));
   }

   [TestMethod]
   [ExpectedException(typeof(ArgumentException))]
   public void Constructor_ShouldThrowException_WhenApiKeyIsNullOrWhiteSpace()
   {
      // Act
      new GitScribeCommitAssistant(m_repositoryManagerMock.Object, new("endpoint", string.Empty, "deploymentName", "modelId"));
   }

   [TestMethod]
   [ExpectedException(typeof(ArgumentException))]
   public void Constructor_ShouldThrowException_WhenDeploymentNameIsNullOrWhiteSpace()
   {
      // Act
      new GitScribeCommitAssistant(m_repositoryManagerMock.Object, new("endpoint", "apiKey", string.Empty, "modelId"));
   }

   [TestMethod]
   [ExpectedException(typeof(ArgumentException))]
   public void Constructor_ShouldThrowException_WhenModelIdIsNullOrWhiteSpace()
   {
      // Act
      new GitScribeCommitAssistant(m_repositoryManagerMock.Object, new("endpoint", "apiKey", "deploymentName", string.Empty));
   }

   [TestMethod]
   [ExpectedException(typeof(ArgumentNullException))]
   public void Constructor_ShouldThrowException_WhenRepositoryManagerIsNull()
   {
      // Act
      new GitScribeCommitAssistant(default!, new("endpoint", "apiKey", "deploymentName", "modelId"));
   }

   [TestMethod]
   [DataRow(FileStatus.ModifiedInWorkdir)]
   [DataRow(FileStatus.RenamedInWorkdir)]
   [DataRow(FileStatus.TypeChangeInWorkdir)]
   [DataRow(FileStatus.NewInWorkdir)]
   [DataRow(FileStatus.DeletedFromWorkdir)]
   public void CollectPatchContent_WithWorkDirFileStatus_ShouldReturnEmptyPatchContent(FileStatus fileStatus)
   {
      // Arrange
      var patchFiles = GetTestPatchFiles();
      var mockPatches = CreateMockPatchesFromFiles(patchFiles);
      var expectedContent = CombinePatchContent(mockPatches);

      var statusEntryMock = CreateMockStatusEntry(fileStatus);
      var repositoryManagerMock = new Mock<IRepositoryManager>();
      repositoryManagerMock.Setup(m => m.RetrieveStatus()).Returns(new[] { statusEntryMock });

      var sut = new GitScribeCommitAssistant(repositoryManagerMock.Object, new("endpoint", "apiKey", "deploymentName", "modelId"));

      // Act
      var result = sut.CollectPatchContent();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("", result);
   }

   [TestMethod]
   [DataRow(FileStatus.ModifiedInIndex)]
   [DataRow(FileStatus.RenamedInIndex)]
   [DataRow(FileStatus.TypeChangeInIndex)]
   [DataRow(FileStatus.NewInIndex)]
   [DataRow(FileStatus.DeletedFromIndex)]
   public void CollectPatchContent_WithIndexFileStatus_ShouldReturnCombinedPatchContent(FileStatus fileStatus)
   {
      // Arrange
      var patchFiles = GetTestPatchFiles();
      var mockPatches = CreateMockPatchesFromFiles(patchFiles);
      var expectedContent = CombinePatchContent(mockPatches);

      var statusEntryMock = CreateMockStatusEntry(fileStatus);
      var repositoryManagerMock = new Mock<IRepositoryManager>();
      repositoryManagerMock.Setup(m => m.RetrieveStatus()).Returns(new[] { statusEntryMock });
      repositoryManagerMock.Setup(m => m.GetPatches(statusEntryMock)).Returns(mockPatches);
      repositoryManagerMock.Setup(m => m.GetPatchContent(mockPatches)).Returns(expectedContent);

      var sut = new GitScribeCommitAssistant(repositoryManagerMock.Object, new("endpoint", "apiKey", "deploymentName", "modelId"));

      // Act
      var result = sut.CollectPatchContent();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(expectedContent.TrimEnd(), result.TrimEnd());
   }


   [TestMethod]
   public void GenerateCommitMessageAsync_WithPatches_GeneratesCorrectCommitMessage()
   {
      var expected = GetPromptTemplate();
      var patchFiles = GetTestPatchFiles();
      var mockPatches = CreateMockPatchesFromFiles(patchFiles);
      var expectedContent = CombinePatchContent(mockPatches);
      var repositoryManagerMock = new Mock<IRepositoryManager>();

      var sut = new GitScribeCommitAssistant(repositoryManagerMock.Object, new("endpoint", "apiKey", "deploymentName", "modelId"));
      
      var result = sut.GeneratePromptMessage(expectedContent);

      Assert.IsNotNull(result);
      Assert.AreEqual(expected, result);
   }

   // Helper method to get test patch file paths
   private static string[] GetTestPatchFiles()
   {
      string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
      var patchFiles = new[]
      {
        Path.Combine(projectDirectory, "TestData", "Patch1.txt"),
        Path.Combine(projectDirectory, "TestData", "Patch2.txt")
      };

      foreach (var file in patchFiles)
      {
         if (!File.Exists(file))
         {
            throw new FileNotFoundException($"Test patch file not found: {file}");
         }
      }

      return patchFiles;
   }

   // Helper method to get PromptTemplate Content
   private static string GetPromptTemplate()
   {
      string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
      var file = Path.Combine(projectDirectory, "TestData", "PromptTemplate.txt");
      
      if (!File.Exists(file))
         throw new FileNotFoundException($"Test patch file not found: {file}");

      return File.ReadAllText(file);
   }

   // Helper to create mock status entries
   private static StatusEntry CreateMockStatusEntry(FileStatus status)
   {
      var mock = new Mock<StatusEntry>();
      mock.Setup(s => s.State).Returns(status);
      return mock.Object;
   }

   // Helper to create mock patches from files
   private static List<Patch> CreateMockPatchesFromFiles(string[] filePaths)
   {
      var patches = new List<Patch>();

      foreach (var filePath in filePaths)
      {
         var mockPatch = new Mock<Patch>();
         mockPatch.Setup(p => p.Content).Returns(File.ReadAllText(filePath));
         patches.Add(mockPatch.Object);
      }

      return patches;
   }

   // Helper to combine patch content
   private static string CombinePatchContent(List<Patch> patches)
   {
      var stringBuilder = new StringBuilder();
      foreach (var patch in patches)
      {
         stringBuilder.AppendLine(patch.Content);
      }
      return stringBuilder.ToString();
   }
}