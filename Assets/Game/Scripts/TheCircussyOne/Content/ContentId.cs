using System.Text;

namespace TheCircussyOne.Content
{
    public readonly struct ContentId
    {
        public ContentId(string value)
        {
            Value = Normalize(value);
        }

        public string Value { get; }
        public bool IsValid => IsValidValue(Value);

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            bool previousWasUnderscore = false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = char.ToLowerInvariant(value[i]);
                bool isAlphaNumeric = c is >= 'a' and <= 'z' || c is >= '0' and <= '9';
                if (isAlphaNumeric)
                {
                    builder.Append(c);
                    previousWasUnderscore = false;
                    continue;
                }

                if ((c == '_' || c == '-' || char.IsWhiteSpace(c)) && builder.Length > 0 && !previousWasUnderscore)
                {
                    builder.Append('_');
                    previousWasUnderscore = true;
                }
            }

            while (builder.Length > 0 && builder[^1] == '_')
            {
                builder.Length--;
            }

            return builder.ToString();
        }

        public static bool IsValidValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value[0] is < 'a' or > 'z' || value[^1] == '_')
            {
                return false;
            }

            bool previousWasUnderscore = false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool isLower = c is >= 'a' and <= 'z';
                bool isDigit = c is >= '0' and <= '9';
                if (isLower || isDigit)
                {
                    previousWasUnderscore = false;
                    continue;
                }

                if (c == '_' && !previousWasUnderscore)
                {
                    previousWasUnderscore = true;
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
