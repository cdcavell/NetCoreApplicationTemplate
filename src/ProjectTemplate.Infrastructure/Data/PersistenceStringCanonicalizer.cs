using System.Net;
using System.Text;

namespace ProjectTemplate.Infrastructure.Data;

internal static class PersistenceStringCanonicalizer
{
    private const int _maxDecodePasses = 4;

    internal static string Canonicalize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string current = value.Normalize(NormalizationForm.FormC);

        for (int pass = 0; pass < _maxDecodePasses; pass++)
        {
            string decoded = WebUtility.HtmlDecode(current);

            if (string.Equals(decoded, current, StringComparison.Ordinal))
            {
                return current;
            }

            current = decoded.Normalize(NormalizationForm.FormC);
        }

        return current;
    }
}
