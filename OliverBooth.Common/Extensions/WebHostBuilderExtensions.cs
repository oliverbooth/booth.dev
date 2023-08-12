using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;

namespace OliverBooth.Common.Extensions;

/// <summary>
///     Extension methods for <see cref="IWebHostBuilder" />.
/// </summary>
public static class WebHostBuilderExtensions
{
    /// <summary>
    ///     Adds a certificate to the <see cref="IWebHostBuilder" /> by reading the paths from environment variables.
    /// </summary>
    /// <param name="builder">The <see cref="IWebHostBuilder" />.</param>
    /// <param name="httpsPort">The HTTPS port.</param>
    /// <param name="httpPort">The HTTP port.</param>
    /// <returns>The <see cref="IWebHostBuilder" />.</returns>
    public static IWebHostBuilder AddCertificateFromEnvironment(this IWebHostBuilder builder,
        int httpsPort = 443,
        int httpPort = 80)
    {
        return builder.UseKestrel(options =>
        {
            string certPath = Environment.GetEnvironmentVariable("SSL_CERT_PATH")!;
            if (!File.Exists(certPath))
            {
                options.ListenAnyIP(httpPort);
                return;
            }

            string? keyPath = Environment.GetEnvironmentVariable("SSL_KEY_PATH");
            if (string.IsNullOrWhiteSpace(keyPath) || !File.Exists(keyPath)) keyPath = null;

            options.ListenAnyIP(httpsPort, options =>
            {
                X509Certificate2 cert = CreateCertFromPemFile(certPath, keyPath);
                options.UseHttps(cert);
            });
            return;

            static X509Certificate2 CreateCertFromPemFile(string certPath, string? keyPath)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return X509Certificate2.CreateFromPemFile(certPath, keyPath);

                //workaround for windows issue https://github.com/dotnet/runtime/issues/23749#issuecomment-388231655
                using var cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
                return new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
            }
        });
    }
}
