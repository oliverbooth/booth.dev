using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service for managing contact information.
/// </summary>
public interface IContactService
{
    /// <summary>
    ///     Gets the blacklist.
    /// </summary>
    IReadOnlyCollection<IBlacklistEntry> GetBlacklist();
}
