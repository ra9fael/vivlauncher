namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace System
{
    internal static class FrameworkStringExtensions
    {
        internal static bool Contains(this string value, char character) => value.IndexOf(character) >= 0;
        internal static bool StartsWith(this string value, char character) => value.Length > 0 && value[0] == character;
        internal static bool EndsWith(this string value, char character) => value.Length > 0 && value[value.Length - 1] == character;
    }
}
