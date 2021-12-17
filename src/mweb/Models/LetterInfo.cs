namespace Minustar.Website.Models;

public record LetterInfo(
    string Value,
    int Primary,
    short Secondary,
    sbyte Tertiary = 0,
    sbyte Quaternary = 0
    ) : IComparable<LetterInfo>
{
    public int CompareTo(LetterInfo? other)
    {
        if (other is null)
            return 1;

        int r = Primary.CompareTo(other.Primary);
        if (r == 0)
        {
            r = Secondary.CompareTo(other.Secondary);
            if (r == 0)
            {
                r = Tertiary.CompareTo(other.Tertiary);
                if (r == 0)
                {
                    r = Quaternary.CompareTo(other.Quaternary);
                }
            }
        }

        return r;
    }

    public string ToStoredString()
    {
        string byteSequence = EncodeValueString(Value);

        return string.Format(
            "{1}{0}{2:G}{0}{3:G}{0}{4:X2}{0}{5:X2}",
            AsciiEndOfText,
            byteSequence,
            Primary,
            Secondary,
            Tertiary,
            Quaternary
            );
    }

    public static LetterInfo Parse(string? str)
    {
        if (TryParse(str, out var value))
            return value;

        throw new FormatException();
    }

    public static bool TryParse(string? str, out LetterInfo? value)
    {
        var tokens = str?.Split(AsciiEndOfText);

        if (tokens?.Length != 5)
        {
            value = null;
            return false;
        }


        if (!int.TryParse(tokens[1], Integer, InvariantCulture, out int primary))
        {
            value = null;
            return false;
        }

        if (!short.TryParse(tokens[2], Integer, InvariantCulture, out short secondary))
        {
            value = null;
            return false;
        }

        if (!sbyte.TryParse(tokens[3], HexNumber, InvariantCulture, out sbyte tertiary))
        {
            value = null;
            return false;
        }

        if (!sbyte.TryParse(tokens[4], HexNumber, InvariantCulture, out sbyte quaternary))
        {
            value = null;
            return false;
        }

        var decodedValue = DecodeValueString(tokens[0]);

        value = new LetterInfo(decodedValue,
                               primary,
                               secondary,
                               tertiary,
                               quaternary);
        return true;
    }

    private static string DecodeValueString(string str)
    {
        var b = new StringBuilder();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            Console.WriteLine($"{i} {c}");
            if (c == '\\')
            {
                var hex = str.Substring(i + 1, 4);
                i += 4;

                c = (char)ushort.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            b.Append(c);
        }

        return b.ToString();
    }

    private static string EncodeValueString(string value)
    {
        var b = new StringBuilder();
        foreach (var c in value)
        {
            b.Append(c switch
            {
                >= '0' and <= '9' => c.ToString(),
                >= 'A' and <= 'Z' => c.ToString(),
                >= 'a' and <= 'z' => c.ToString(),
                _ => $"\\{(ushort)c:X4}"
            });
        }

        return b.ToString();
    }
}
