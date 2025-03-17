namespace GitScribe.Core;

public record RepositorySettings()
{
   public const string SectionName = "RepositorySettings";

   public List<RepositoryConfig> Repositories { get; set; } = [];
}

public class RepositoryConfig
{
   public required string Id { get; set; }
   public required string Name { get; set; }
   public required string Path { get; set; }
   public bool IsActive { get; set; } = true;
}