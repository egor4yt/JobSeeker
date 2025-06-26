using JobSeeker.WebApi.ObjectStorage.Models;

namespace JobSeeker.WebApi.ObjectStorage;

public interface IObjectStorage
{
    /// <summary>
    ///     Uploads an object to the object storage.
    /// </summary>
    /// <param name="options">The options specifying the request to upload.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    Task PutObjectAsync(PutObjectOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves all object keys recursively from the specified bucket and path in object storage.
    /// </summary>
    /// <param name="options">The options specifying the request to retrieve the objects from.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>List of object keys.</returns>
    Task<List<string>> GetAllObjectsRecursiveAsync(GetAllObjectsOptions options, CancellationToken cancellationToken = default);

    Task<Stream> GetObjectStreamAsync(GetObjectOptions options, CancellationToken cancellationToken = default);

    Task DeleteObjectAsync(DeleteObjectOptions options, CancellationToken cancellationToken = default);
}