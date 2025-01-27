using LibGit2Sharp;
using Moq;
using System.Text;

namespace GitScribe.Core.Tests;

[TestClass]
public class GitScribeServiceTests
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
      new GitScribeService(m_repositoryManagerMock.Object, string.Empty, "apiKey", "deploymentName", "modelId");
   }

   [TestMethod]
   [ExpectedException(typeof(ArgumentException))]
   public void Constructor_ShouldThrowException_WhenApiKeyIsNullOrWhiteSpace()
   {
      // Act
      new GitScribeService(m_repositoryManagerMock.Object, "endpoint", string.Empty, "deploymentName", "modelId");
   }

   [TestMethod]
   [ExpectedException(typeof(ArgumentException))]
   public void Constructor_ShouldThrowException_WhenDeploymentNameIsNullOrWhiteSpace()
   {
      // Act
      new GitScribeService(m_repositoryManagerMock.Object, "endpoint", "apiKey", string.Empty, "modelId");
   }

   [TestMethod]
   [ExpectedException(typeof(ArgumentException))]
   public void Constructor_ShouldThrowException_WhenModelIdIsNullOrWhiteSpace()
   {
      // Act
      new GitScribeService(m_repositoryManagerMock.Object, "endpoint", "apiKey", "deploymentName", string.Empty);
   }

   [TestMethod]
   [ExpectedException(typeof(ArgumentNullException))]
   public void Constructor_ShouldThrowException_WhenRepositoryManagerIsNull()
   {
      // Act
      new GitScribeService(default!, "endpoint", "apiKey", "deploymentName", "modelId");
   }

   [TestMethod]
   public void CollectPatchContent_ShouldReturnCombinedPatchContent()
   {
      // Arrange
      var patchFiles = GetTestPatchFiles();
      var mockPatches = CreateMockPatchesFromFiles(patchFiles);
      var expectedContent = CombinePatchContent(mockPatches);

      var statusEntryMock = CreateMockStatusEntry(FileStatus.ModifiedInIndex);
      var repositoryManagerMock = new Mock<IRepositoryManager>();
      repositoryManagerMock.Setup(m => m.RetrieveStatus()).Returns(new[] { statusEntryMock });
      repositoryManagerMock.Setup(m => m.GetPatches(statusEntryMock)).Returns(mockPatches);
      repositoryManagerMock.Setup(m => m.GetPatchContent(mockPatches)).Returns(expectedContent);

      var sut = new GitScribeService(repositoryManagerMock.Object, "endpoint", "apiKey", "deploymentName", "modelId");

      // Act
      var result = sut.CollectPatchContent();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(expectedContent.TrimEnd(), result.TrimEnd());
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

   //[TestMethod]
   //public void CollectPatchContent_ShouldReturnEmptyString_WhenNoRelevantFileStatus()
   //{
   //   // Arrange
   //   var fileStatusEntries = new List<FileStatusEntry>
   //  {
   //      new FileStatusEntry { State = FileStatus.Unchanged },
   //      new FileStatusEntry { State = FileStatus.Ignored }
   //  };

   //   m_repositoryManagerMock.Setup(m => m.RetrieveStatus()).Returns(fileStatusEntries);

   //   var service = new GitScribeService(m_repositoryManagerMock.Object, "endpoint", "apiKey", "deploymentName", "modelId");

   //   // Act
   //   var result = service.CollectPatchContent();

   //   // Assert
   //   Assert.IsNotNull(result);
   //   Assert.AreEqual(string.Empty, result);
   //}
}