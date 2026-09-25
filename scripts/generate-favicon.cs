#:project ../BoothDotDev/BoothDotDev.csproj

using BoothDotDev.Services;

const string usage = """
    Generates the site's favicon: a tilted gradient tile carrying the owner's initials, drawn by the same code as the
    wordmark on the Open Graph cards.

    Run from the repository root:
        dotnet run scripts/generate-favicon.cs [-- options]

    Options:
        --initials <AB>   Letters to show. Default: taken from MyFirstName and MySurname in Strings.resx.
        --out <dir>       Folder to write into. Default: public/img
        --size <pixels>   Width and height of the icon. Default: 256
    """;

var initials = OgImageService.Initials;
var outDir = Path.Combine("public", "img");
var size = 256;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--initials" when i + 1 < args.Length:
            initials = args[++i];
            break;
        case "--out" when i + 1 < args.Length:
            outDir = args[++i];
            break;
        case "--size" when i + 1 < args.Length && int.TryParse(args[i + 1], out var parsed) && parsed > 0:
            size = parsed;
            i++;
            break;
        default:
            Console.Error.WriteLine(usage);
            return args[i] is "--help" or "-h" ? 0 : 1;
    }
}

if (string.IsNullOrWhiteSpace(initials))
{
    Console.Error.WriteLine("The initials can't be empty.");
    return 1;
}

Directory.CreateDirectory(outDir);
var png = new OgImageService().RenderBrandIcon(initials, size);

var path = Path.Combine(outDir, "favicon.png");
File.WriteAllBytes(path, png);
Console.WriteLine($"Wrote {path} ({initials.ToUpperInvariant()}, {size}x{size})");

return 0;
