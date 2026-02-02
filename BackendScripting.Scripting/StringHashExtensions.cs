
namespace BackendScripting.Scripting;

/// <summary>
/// Hash extensions for <see cref="string"/>
/// </summary>
/// <exclude />
public static class StringHashExtensions
{
    /// <summary>Get the lookup key hash code</summary>
    /// <param name="source">The source value</param>
    /// <returns>The value key hash code, zero on empty value</returns>
    public static int GetScriptHashCode(this string source) =>
        string.IsNullOrWhiteSpace(source) ? 0 : GetHashCode(source);

    private static int GetHashCode(params object[] values)
    {
        // create hash code to store in database
        // the CLR GetScriptHashCode() depends on the framework version
        // see https://stackoverflow.com/a/5155015/15659039
        unchecked // Overflow is fine, just wrap
        {
            var hash = 23;
            foreach (var value in values)
            {
                var stringValue = value.ToString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    continue;
                }
                foreach (var c in stringValue)
                {
                    hash = hash * 31 + c;
                }
            }
            return hash;
        }
    }
}