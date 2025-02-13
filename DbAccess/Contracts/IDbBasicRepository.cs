﻿using DbAccess.Models;

namespace DbAccess.Contracts;

/// <summary>
/// Represents a basic repository that provides data access operations for entities of type <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IDbBasicRepository<T>
{
    /// <summary>
    /// Retrieves a collection of entities based on the provided request options.
    /// </summary>
    /// <param name="options">The request options such as paging, language, or as-of date. If null, default options are applied.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of entities.</returns>
    Task<IEnumerable<T>> Get(RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="options">The request options, such as language or as-of date. If null, default options are applied.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.
    /// </returns>
    Task<T?> Get(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a collection of entities that match the criteria specified by a <see cref="GenericFilterBuilder{T}"/>.
    /// </summary>
    /// <param name="filterBuilder">A builder object that specifies the filter criteria.</param>
    /// <param name="options">The request options, such as paging or language settings.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of matching entities.</returns>
    Task<IEnumerable<T>> Get(GenericFilterBuilder<T> filterBuilder, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a collection of entities that match the provided list of filters.
    /// </summary>
    /// <param name="filters">A collection of filter criteria.</param>
    /// <param name="options">The request options, such as paging, language, or as-of date.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of matching entities.</returns>
    Task<IEnumerable<T>> Get(IEnumerable<GenericFilter> filters, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk ingest operation by importing a list of entities into the database.
    /// </summary>
    /// <param name="data">The list of entities to import.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of entities successfully ingested.
    /// </returns>
    Task<int> Ingest(List<T> data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new entity into the database.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of rows affected.
    /// </returns>
    Task<int> Create(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates an entity in the database. If the entity already exists, it will be updated.
    /// </summary>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of rows affected.
    /// </returns>
    Task<int> Upsert(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the database identified by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to update.</param>
    /// <param name="entity">The updated entity data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of rows affected.
    /// </returns>
    Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates specific properties of an existing entity in the database using a list of generic parameters.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to update.</param>
    /// <param name="parameters">A list of generic parameters representing the properties to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of rows affected.
    /// </returns>
    Task<int> Update(Guid id, List<GenericParameter> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the database identified by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of rows affected.
    /// </returns>
    Task<int> Delete(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a translation entry for the specified entity in a different language.
    /// </summary>
    /// <param name="obj">The entity for which to create a translation.</param>
    /// <param name="language">The language code for the translation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of rows affected.
    /// </returns>
    Task<int> CreateTranslation(T obj, string language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the translation for an entity in a specified language.
    /// </summary>
    /// <param name="id">The unique identifier of the entity whose translation is to be updated.</param>
    /// <param name="obj">The entity containing the updated translation data.</param>
    /// <param name="language">The language code for which the translation is to be updated.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of rows affected.
    /// </returns>
    Task<int> UpdateTranslation(Guid id, T obj, string language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new instance of a <see cref="GenericFilterBuilder{T}"/> for constructing filter criteria for queries.
    /// </summary>
    /// <returns>A new <see cref="GenericFilterBuilder{T}"/> instance for building filter conditions for the entity type <typeparamref name="T"/>.</returns>
    GenericFilterBuilder<T> CreateFilterBuilder();
}
