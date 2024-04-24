using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabApp;

/// <summary>
/// Support class for method invoking directly from the XAML code in FileBackupView.xaml
/// This could be done using converters, but I like to show different techniques offering the same result.
/// </summary>
public static class AssemblyHelper
{
    /// <summary>
    /// Return the declaring type's version.
    /// </summary>
    /// <remarks>Includes string formatting.</remarks>
    public static string GetVersion()
    {
        var ver = App.GetCurrentAssemblyVersion();
        return $"Version {ver}";
    }

    /// <summary>
    /// Return the declaring type's namespace.
    /// </summary>
    public static string? GetNamespace()
    {
        var assembly = App.GetCurrentNamespace();
        return assembly ?? "Unknown";
    }

    /// <summary>
    /// Return the declaring type's assembly name.
    /// </summary>
    public static string? GetAssemblyName()
    {
        var assembly = App.GetCurrentAssemblyName()?.Split(',')[0].SeparateCamelCase();
        return assembly ?? "Unknown";
    }
}
