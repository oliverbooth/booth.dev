namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a programming language.
/// </summary>
public static class ProgrammingLanguage
{
    private static readonly Dictionary<string, string> KeyToNameMap = new()
    {
        ["cs"] = "C#",
        ["vb"] = "Visual Basic",
        ["fs"] = "F#",
        ["java"] = "Java",
        ["cpp"] = "C++",
        ["kt"] = "Kotlin",
        ["pawn"] = "Pawn",
        ["unity"] = "Unity",
        ["unreal"] = "Unreal Engine",
        ["gd"] = "Godot",
    };

    /// <summary>
    ///     Gets the name of the programming language from the shorthand key.
    /// </summary>
    /// <param name="shorthand">The shorthand key of the programming language.</param>
    /// <returns>The name of the programming language, or the shorthand key if not found.</returns>
    public static string GetNameFromShorthand(string shorthand)
    {
        return KeyToNameMap.TryGetValue(shorthand, out var name) ? name : shorthand;
    }

    /// <summary>
    ///     Gets the shorthand key of the programming language from the name.
    /// </summary>
    /// <param name="name">The name of the programming language.</param>
    /// <returns>The shorthand key of the programming language, or the name if not found.</returns>
    public static string GetShorthandFromName(string name)
    {
        foreach (var pair in KeyToNameMap)
        {
            if (pair.Value == name)
            {
                return pair.Key;
            }
        }

        return name;
    }
}
