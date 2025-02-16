using System.Security.Cryptography.X509Certificates;

namespace BoothDotDev.Extensions;

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
            if (string.IsNullOrWhiteSpace(certPath))
            {
                Console.WriteLine("Certificate path not specified. Using HTTP");
                options.ListenAnyIP(httpPort);
                return;
            }

            if (!File.Exists(certPath))
            {
                Console.Error.WriteLine("Certificate not found. Using HTTP");
                options.ListenAnyIP(httpPort);
                return;
            }

            string? keyPath = Environment.GetEnvironmentVariable("SSL_KEY_PATH");
            if (string.IsNullOrWhiteSpace(keyPath))
            {
                Console.WriteLine("Certificate found, but no key provided. Using certificate only");
                keyPath = null;
            }
            else if (!File.Exists(keyPath))
            {
                Console.Error.WriteLine("Certificate found, but the provided key was not. Using certificate only");
                keyPath = null;
            }

            Console.WriteLine($"Using HTTPS with certificate found at {certPath}:{keyPath}");
            var certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
            options.ListenAnyIP(httpsPort, configure => configure.UseHttps(certificate));
        });
    }
}
