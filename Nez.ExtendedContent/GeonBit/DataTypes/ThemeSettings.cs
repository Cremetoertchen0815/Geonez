namespace Nez.ExtendedContent.DataTypes;

/// <summary>
///     General data / settings about a UI theme.
///     Loaded from the theme data xml file.
/// </summary>
public class ThemeSettings
{
    /// <summary>Theme author name.</summary>
    public string AuthorName;

    /// <summary>Theme additional credits.</summary>
    public string Credits;

    /// <summary>Theme description.</summary>
    public string Description;

    /// <summary>Theme license.</summary>
    public string License;

    /// <summary>Theme project URL.</summary>
    public string RepoUrl;

    /// <summary>Name fot he theme.</summary>
    public string ThemeName;

    /// <summary>Theme version.</summary>
    public string Version;
}