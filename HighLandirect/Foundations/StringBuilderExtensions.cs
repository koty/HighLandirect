using System;
using System.Text;

namespace HighLandirect.Foundations
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendInNewLine(this StringBuilder stringBuilder, string value)
        {
            if (stringBuilder == null) { throw new ArgumentNullException("stringBuilder"); }

            if (stringBuilder.Length == 0)
            {
                stringBuilder.Append(value);
            }
            else
            {
                stringBuilder.Append(Environment.NewLine + value);
            }
            return stringBuilder;
        }
    }
}
