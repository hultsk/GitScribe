namespace GitScribe.Core;

public record RepositorySettings()
{
   public const string SectionName = "RepositorySettings";

   public List<RepositoryConfig> Repositories { get; set; } = [];
}

public record RepositoryConfig(string Name, string Path);