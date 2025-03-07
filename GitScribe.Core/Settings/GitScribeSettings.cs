namespace GitScribe.Core;

public record GitScribeSettings(string Endpoint, string ApiKey, string DeploymentName, string ModelId)
{
   public const string SectionName = "GitScribeSettings";
}
