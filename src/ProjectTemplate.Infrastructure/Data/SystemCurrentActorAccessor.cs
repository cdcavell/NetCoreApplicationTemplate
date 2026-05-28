namespace ProjectTemplate.Infrastructure.Data;

/// <summary>
/// Provides a default actor value for non-HTTP infrastructure consumers such as workers,
/// CLI tools, test harnesses, and system modules.
/// </summary>
public sealed class SystemCurrentActorAccessor : ICurrentActorAccessor
{
    public const string ActorName = "System";

    public string CurrentActor => ActorName;
}
