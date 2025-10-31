using System.Globalization;
using System.Text;

namespace SkinCareSystem.Services.Rag;

internal static class PgVectorHelper
{
    public static string ToPgVectorText(IReadOnlyList<float> values)
    {
        if (values.Count == 0)
        {
            return "[]";
        }

        var builder = new StringBuilder(values.Count * 8);
        builder.Append('[');

        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }

            builder.Append(values[i].ToString(CultureInfo.InvariantCulture));
        }

        builder.Append(']');
        return builder.ToString();
    }
}
