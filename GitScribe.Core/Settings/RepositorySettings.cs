namespace GitScribe.Core;

public record RepositorySettings()
{
   public const string SectionName = "RepositorySettings";

   public List<RepositoryConfig> Repositories { get; set; } = [];
}

public record RepositoryConfig(string Id, string Name, string Path)
{
   public bool IsActive { get; set; } = true;
}