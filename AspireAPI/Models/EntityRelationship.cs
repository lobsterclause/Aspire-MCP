using Microsoft.Extensions.Logging; // Added missing using directive
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspireAPI;

/// <summary>
/// Represents the result of an entity relationship exploration
/// </summary>
public class EntityRelationshipResult
{
    /// <summary>
    /// The type of entity (TimeEntries, Contacts, etc.)
    /// </summary>
    public string EntityType { get; set; }
    
    /// <summary>
    /// The ID of the entity
    /// </summary>
    public string EntityId { get; set; }
    
    /// <summary>
    /// The name or title of the entity
    /// </summary>
    public string EntityName { get; set; }
    
    /// <summary>
    /// Detailed properties of the entity
    /// </summary>
    public Dictionary<string, object> EntityDetails { get; set; }
    
    /// <summary>
    /// Relationships to other entities
    /// </summary>
    public List<Relationship> Relationships { get; set; } = new();
}

/// <summary>
/// Represents a relationship between entities
/// </summary>
public class Relationship
{
    /// <summary>
    /// The type of relationship (parent, child, related)
    /// </summary>
    public string RelationshipType { get; set; }
    
    /// <summary>
    /// The type of the related entity
    /// </summary>
    public string RelatedEntityType { get; set; }
    
    /// <summary>
    /// The ID of the related entity
    /// </summary>
    public string RelatedEntityId { get; set; }
    
    /// <summary>
    /// The name or title of the related entity
    /// </summary>
    public string RelatedEntityName { get; set; }
    
    /// <summary>
    /// Detailed properties of the related entity
    /// </summary>
    public Dictionary<string, object> RelatedEntityDetails { get; set; }
    
    /// <summary>
    /// Nested relationships (for recursive exploration)
    /// </summary>
    public List<Relationship> NestedRelationships { get; set; }
}

/// <summary>
/// Interface for entity finders
/// </summary>
public interface IEntityFinder
{
    /// <summary>
    /// Gets entity details by ID
    /// </summary>
    Task<EntityRelationshipResult> GetEntityAsync(
        string entityId, 
        bool includeDetails, 
        CancellationToken cancellationToken);
}

/// <summary>
/// Interface for relationship finders
/// </summary>
public interface IRelationshipFinder
{
    /// <summary>
    /// Finds relationships for an entity
    /// </summary>
    Task<List<Relationship>> FindRelationshipsAsync(
        string entityId, 
        List<string> relationshipTypes, 
        bool includeDetails, 
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for entity finders
/// </summary>
public abstract class EntityFinderBase : IEntityFinder
{
    protected readonly AspireApiService AspireApi;
    protected readonly ILogger Logger;
    
    protected EntityFinderBase(AspireApiService aspireApi, ILogger logger)
    {
        AspireApi = aspireApi;
        Logger = logger;
    }
    
    /// <summary>
    /// Gets entity details by ID
    /// </summary>
    public abstract Task<EntityRelationshipResult> GetEntityAsync(
        string entityId, 
        bool includeDetails, 
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for relationship finders
/// </summary>
public abstract class RelationshipFinderBase : IRelationshipFinder
{
    protected readonly AspireApiService AspireApi;
    protected readonly ILogger Logger;
    
    protected RelationshipFinderBase(AspireApiService aspireApi, ILogger logger)
    {
        AspireApi = aspireApi;
        Logger = logger;
    }
    
    /// <summary>
    /// Finds relationships for an entity
    /// </summary>
    public abstract Task<List<Relationship>> FindRelationshipsAsync(
        string entityId, 
        List<string> relationshipTypes, 
        bool includeDetails, 
        CancellationToken cancellationToken);
}