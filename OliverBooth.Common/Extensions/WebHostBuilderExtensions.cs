using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;

namespace OliverBooth.Common.Extensions;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder AddCertificateFromEnvironment(this IWebHostBuilder builder)
    {
        return builder.UseKestrel(options =>
        {
            string certPath = Environment.GetEnvironmentVariable("SSL_CERT_PATH")!;
            if (!File.Exists(certPath))
            {
                options.ListenAnyIP(5049);
                return;
            }

            string? keyPath = Environment.GetEnvironmentVariable("SSL_KEY_PATH");
            if (string.IsNullOrWhiteSpace(keyPath) || !File.Exists(keyPath)) keyPath = null;

            options.ListenAnyIP(2845, options =>
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
