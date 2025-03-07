namespace GitScribe.Core;

public record RepositorySettings(string RepositoryPath)
{
   public const string SectionName = "RepositorySettings";
}