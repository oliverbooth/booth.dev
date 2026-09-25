using BoothDotDev.Data;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extensions for <see cref="ProjectType" />.
/// </summary>
public static class ProjectTypeExtensions
{
    /// <param name="type">The type of project.</param>
    extension(ProjectType type)
    {
        /// <summary>
        ///     Gets the short lowercase name a project of the type is labelled with.
        /// </summary>
        /// <value>The label, such as <c>game</c> or <c>library</c>.</value>
        public string Label
        {
            get => type.ToString().ToLowerInvariant();
        }
    }
}
