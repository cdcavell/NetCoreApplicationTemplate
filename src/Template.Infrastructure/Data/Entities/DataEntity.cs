using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Infrastructure.Data.Entities;

/// <summary>
/// DataEntity Interface
/// </summary>
public interface IDataEntity<T>
{
    /// <summary>
    /// Primary key of the entity, generated as a new Guid when the record is created. It is used to uniquely identify each record in the database.
    /// </summary>
    Guid Guid { get; set; }

    /// <summary>
    /// Boolean property that indicates whether the entity is new (i.e., it has not been saved to the database yet). It returns true if the Guid is empty, which means the entity has not been assigned a unique identifier and is considered new.
    /// </summary>
    bool IsNew { get; }

    /// <summary>
    /// A method that adds or updates the entity in the database context. If the entity is new (i.e., it has not been saved to the database yet), it will be added to the context. If the entity already exists (i.e., it has a non-empty Guid), it will be updated in the context.
    /// </summary>
    /// <param name="dbContext">
    /// Reference to the database context (TemplateDbContext) that is used to interact with the database. This parameter allows the method to perform database operations such as adding or updating the entity in the context.
    /// </param>
    void AddUpdate(TemplateDbContext dbContext);

    /// <summary>
    /// A method that adds or updates the entity in the database context. If the entity is new (i.e., it has not been saved to the database yet), it will be added to the context. If the entity already exists (i.e., it has a non-empty Guid), it will be updated in the context.
    /// </summary>
    /// <param name="dbContext">
    /// Reference to the database context (TemplateDbContext) that is used to interact with the database. This parameter allows the method to perform database operations such as adding or updating the entity in the context.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token that can be used to cancel the asynchronous operation. This parameter allows the method to be cancelled if needed, for example, if the operation takes too long or if the user decides to cancel it. The method will check for cancellation requests and respond accordingly, allowing for better control over long-running operations.
    /// </param>
    Task AddUpdateAsync(TemplateDbContext dbContext, CancellationToken cancellationToken = default);

    /// <summary>
    /// A method that removes the entity from the database context. This method is used to delete the entity from the database when it is no longer needed.
    /// </summary>
    /// <param name="dbContext">
    /// Reference to the database context (TemplateDbContext) that is used to interact with the database. This parameter allows the method to perform database operations such as adding or updating the entity in the context.
    /// </param>
    void Remove(TemplateDbContext dbContext);

    /// <summary>
    /// A method that removes the entity from the database context. This method is used to delete the entity from the database when it is no longer needed.
    /// </summary>
    /// <param name="dbContext">
    /// Reference to the database context (TemplateDbContext) that is used to interact with the database. This parameter allows the method to perform database operations such as adding or updating the entity in the context.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token that can be used to cancel the asynchronous operation. This parameter allows the method to be cancelled if needed, for example, if the operation takes too long or if the user decides to cancel it. The method will check for cancellation requests and respond accordingly, allowing for better control over long-running operations.
    /// </param>
    Task RemoveAsync(TemplateDbContext dbContext, CancellationToken cancellationToken = default);

    /// <summary>
    /// A method that compares the current entity with another entity of the same type to determine if they are equal. This method is typically used to compare two entities based on their properties and values to check if they represent the same data in the database.
    /// </summary>
    /// <param name="obj">
    /// The entity to compare with the current entity. This parameter is of the same type as the current entity and is used to perform the equality comparison. The method will return true if the two entities are considered equal based on their properties and values, and false otherwise.
    /// </param>
    /// <returns>
    /// Boolean value indicating whether the current entity is equal to the specified entity. The method will return true if the two entities are considered equal based on their properties and values, and false otherwise.
    /// </returns>
    bool Equals(T obj);
}

public abstract partial class DataEntity<T> : IDataEntity<DataEntity<T>> where T : DataEntity<T>
{
    /// <summary>
    /// Primary key of the entity, generated as a new Guid when the record is created. It is used to uniquely identify each record in the database.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; } = Guid.Empty;

    /// <summary>
    /// Boolean property that indicates whether the entity is new (i.e., it has not been saved to the database yet). It returns true if the Guid is empty, which means the entity has not been assigned a unique identifier and is considered new.
    /// </summary>
    [NotMapped]
    public bool IsNew => Guid == Guid.Empty;


    /// <summary>
    /// A method that adds or updates the entity in the database context. If the entity is new (i.e., it has not been saved to the database yet), it will be added to the context. If the entity already exists (i.e., it has a non-empty Guid), it will be updated in the context.
    /// </summary>
    /// <param name="dbContext">
    /// Reference to the database context (TemplateDbContext) that is used to interact with the database. This parameter allows the method to perform database operations such as adding or updating the entity in the context.
    /// </param>
    public virtual void AddUpdate(TemplateDbContext dbContext)
    {
        if (IsNew)
        {
            Guid = Guid.NewGuid();
            dbContext.Add(this);
        }
        else
        {
            dbContext.Update(this);
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            dbContext.SaveChanges();
        }
    }

    /// <summary>
    /// A method that adds or updates the entity in the database context. If the entity is new (i.e., it has not been saved to the database yet), it will be added to the context. If the entity already exists (i.e., it has a non-empty Guid), it will be updated in the context.
    /// </summary>
    /// <param name="dbContext">
    /// Reference to the database context (TemplateDbContext) that is used to interact with the database. This parameter allows the method to perform database operations such as adding or updating the entity in the context.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token that can be used to cancel the asynchronous operation. This parameter allows the method to be cancelled if needed, for example, if the operation takes too long or if the user decides to cancel it. The method will check for cancellation requests and respond accordingly, allowing for better control over long-running operations.
    /// </param>
    public virtual async Task AddUpdateAsync(TemplateDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (IsNew)
        {
            Guid = Guid.NewGuid();
            await dbContext.AddAsync(this, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            dbContext.Update(this);
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// A method that removes the entity from the database context. This method is used to delete the entity from the database when it is no longer needed.
    /// </summary>
    /// <param name="dbContext">
    /// Reference to the database context (TemplateDbContext) that is used to interact with the database. This parameter allows the method to perform database operations such as adding or updating the entity in the context.
    /// </param>
    public virtual void Remove(TemplateDbContext dbContext)
    {
        dbContext.Attach(this);
        dbContext.Remove(this);
        if (dbContext.ChangeTracker.HasChanges())
        {
            dbContext.SaveChanges();
        }
    }

    /// <summary>
    /// A method that removes the entity from the database context. This method is used to delete the entity from the database when it is no longer needed.
    /// </summary>
    /// <param name="dbContext">
    /// Reference to the database context (TemplateDbContext) that is used to interact with the database. This parameter allows the method to perform database operations such as adding or updating the entity in the context.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token that can be used to cancel the asynchronous operation. This parameter allows the method to be cancelled if needed, for example, if the operation takes too long or if the user decides to cancel it. The method will check for cancellation requests and respond accordingly, allowing for better control over long-running operations.
    /// </param>
    public virtual async Task RemoveAsync(TemplateDbContext dbContext, CancellationToken cancellationToken = default)
    {
        dbContext.Attach(this);
        dbContext.Remove(this);
        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// A method that compares the current entity with another entity of the same type to determine if they are equal. This method is typically used to compare two entities based on their properties and values to check if they represent the same data in the database.
    /// </summary>
    /// <param name="obj">
    /// The entity to compare with the current entity. This parameter is of the same type as the current entity and is used to perform the equality comparison. The method will return true if the two entities are considered equal based on their properties and values, and false otherwise.
    /// </param>
    /// <returns>
    /// Boolean value indicating whether the current entity is equal to the specified entity. The method will return true if the two entities are considered equal based on their properties and values, and false otherwise.
    /// </returns>
    public virtual bool Equals(DataEntity<T> obj)
    {
        return Equals(this, obj);
    }
}
