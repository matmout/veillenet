namespace VeilleNet.Models;

public class SkillPageData
{
    public string ReviewLabel { get; set; } = string.Empty;
    public string CatalogNote { get; set; } = string.Empty;
    public string ArchitectureNote { get; set; } = string.Empty;
    public List<SkillLifecycleStep> Lifecycle { get; set; } = new();
    public List<SkillArchitecturePattern> Architectures { get; set; } = new();
    public SkillRefreshWorkflow RefreshWorkflow { get; set; } = new();
    public List<SkillCategory> Categories { get; set; } = new();
}

public class SkillLifecycleStep
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class SkillArchitecturePattern
{
    public string Ecosystem { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PackagingStyle { get; set; } = string.Empty;
    public string OfficialUrl { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string FileTree { get; set; } = string.Empty;
    public List<string> TypicalFiles { get; set; } = new();
}

public class SkillRefreshWorkflow
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Cadence { get; set; } = string.Empty;
    public List<string> Signals { get; set; } = new();
    public List<string> Steps { get; set; } = new();
    public List<SkillSource> Sources { get; set; } = new();
}

public class SkillSource
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
}

public class SkillCategory
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColorClass { get; set; } = string.Empty;
    public List<Skill> Skills { get; set; } = new();
}

public class Skill
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Ecosystem { get; set; } = string.Empty;
    public string GuideUrl { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string PackagingStyle { get; set; } = string.Empty;
    public string PrimaryUseCase { get; set; } = string.Empty;
    public string AdoptionSignal { get; set; } = string.Empty;
}
