using System.Diagnostics;

namespace BoothDotDev.Extensions;

/// <summary>
///     Defines utility methods for working with files and directories.
/// </summary>
public static class FileUtility
{
    /// <summary>
    ///     Extension methods for <see cref="FileInfo" />.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    extension(FileInfo fileInfo)
    {
        /// <summary>
        ///    Changes the owner of the file represented by the specified <see cref="FileInfo" />.
        /// </summary>
        /// <param name="owner">The new owner username.</param>
        /// <param name="group">The new group name (optional).</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="owner" /> is <see langword="null" />.</exception>
        /// <exception cref="PlatformNotSupportedException">Changing file ownership is not supported on Windows.</exception>
        /// <exception cref="InvalidOperationException">Failed to change ownership of the specified file.</exception>
        public async Task ChangeOwnerAsync(string owner, string? group = null)
        {
            await FileUtility.ChangeOwnerAsync(fileInfo.FullName, owner, group);
        }
    }

    /// <summary>
    ///     Extension methods for <see cref="DirectoryInfo" />.
    /// </summary>
    /// <param name="directoryInfo">The directory.</param>
    extension(DirectoryInfo directoryInfo)
    {
        /// <summary>
        ///    Changes the owner of the directory represented by the specified <see cref="DirectoryInfo" />.
        /// </summary>
        /// <param name="owner">The new owner username.</param>
        /// <param name="group">The new group name (optional).</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="owner" /> is <see langword="null" />.</exception>
        /// <exception cref="PlatformNotSupportedException">Changing directory ownership is not supported on Windows.</exception>
        /// <exception cref="InvalidOperationException">Failed to change ownership of the specified directory.</exception>
        public async Task ChangeOwnerAsync(string owner, string? group = null)
        {
            await FileUtility.ChangeOwnerAsync(directoryInfo.FullName, owner, group);
        }
    }

    /// <summary>
    ///     Changes the owner of the specified file or directory.
    /// </summary>
    /// <param name="path">The path to the file or directory.</param>
    /// <param name="owner">The new owner username.</param>
    /// <param name="group">The new group name (optional).</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="path" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="owner" /> is <see langword="null" />.</para>
    /// </exception>
    /// <exception cref="PlatformNotSupportedException">Changing file ownership is not supported on Windows.</exception>
    /// <exception cref="InvalidOperationException">Failed to change ownership of the specified file or directory.</exception>
    public static async Task ChangeOwnerAsync(string path, string owner, string? group = null)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (owner is null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        if (OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Changing file ownership is not supported on Windows.");
        }

        string args = group is null ? $"{owner} \"{path}\"" : $"{owner}:{group} \"{path}\"";
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "chown",
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = processStartInfo;
        process.Start();

        await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to change ownership of '{path}': {error}");
        }
    }
}
