namespace Template.Infrastructure.Data;

/// <summary>
/// Accessor for retrieving the current actor (e.g., user or system component) responsible for changes in the database context.
/// </summary>
public interface ICurrentActorAccessor
{
    string CurrentActor { get; }
}

