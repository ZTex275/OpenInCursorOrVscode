using System.Globalization;
using System.Reflection;
using System.Resources;

namespace OpenInCursorOrVscode
{
    internal static class LocalizedStrings
    {
        private static readonly ResourceManager ResourceManager =
            new ResourceManager("VSPackage", typeof(LocalizedStrings).Assembly);

        public static string CommandTitle => GetString("101");

        public static string FileNotSelected => GetString("FileNotSelected");

        public static string EditorsNotFound => GetString("EditorsNotFound");

        public static string FileNotFound(string path)
        {
            return string.Format(CultureInfo.CurrentUICulture, GetString("FileNotFound"), path);
        }

        public static string LaunchFailed(string editorName, string details)
        {
            return string.Format(CultureInfo.CurrentUICulture, GetString("LaunchFailed"), editorName, details);
        }

        private static string GetString(string name)
        {
            return ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;
        }
    }
}
