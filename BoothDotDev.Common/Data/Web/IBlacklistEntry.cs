namespace BoothDotDev.Common.Data.Web;

/// <summary>
///     Represents an entry in the blacklist.
/// </summary>
public interface IBlacklistEntry
{
    /// <summary>
    ///     Gets the email address of the entry.
    /// </summary>
    /// <value>The email address of the entry.</value>
    string EmailAddress { get; }

    /// <summary>
    ///     Gets the name of the entry.
    /// </summary>
    /// <value>The name of the entry.</value>
    string Name { get; }

    /// <summary>
    ///     Gets the reason for the entry.
    /// </summary>
    /// <value>The reason for the entry.</value>
    string Reason { get; }
}
