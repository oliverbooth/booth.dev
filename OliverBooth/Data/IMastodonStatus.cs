namespace OliverBooth.Data;

public interface IMastodonStatus
{
    /// <summary>
    ///     Gets the content of the status.
    /// </summary>
    /// <value>The content.</value>
    string Content { get; }
    
    /// <summary>
    ///     Gets the date and time at which this status was posted.
    /// </summary>
    /// <value>The post timestamp.</value>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    ///     Gets the original URI of the status.
    /// </summary>
    /// <value>The original URI.</value>
    Uri OriginalUri { get; }
}