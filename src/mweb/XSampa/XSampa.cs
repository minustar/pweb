namespace Minustar.Website.XSampa;

public static class XSampa
{
    public static string? ParseXSampa(this string? src, XSampaOptions? options = null)
    {
        if (src is null)
            return null;

        // Storing options.
        var actualOptions = options ?? DefaultOptions;

        // This function will return the character
        // from an XSampaCharacter according to the
        // options passed above.
        // NOTE: If the "dialect" property is empty
        // we return the XSampa property.
        Func<XSampaCharacter, string> selector =
            actualOptions.Dialect switch
            {
                Cxs => (XSampaCharacter c) => c.Cxs ?? c.XSampa,
                ZSampa => (XSampaCharacter c) => c.ZSampa ?? c.XSampa,
                _ => (XSampaCharacter c) => c.XSampa
            };


        // Creating a string builder.
        var output = new StringBuilder();

        // This main loop will consume the src one
        // character at a point.
        int offset = 0;
        while (offset < src.Length)
        {
            // This special case will handle the INVERTED
            // BREVE used for affricates and co-articulated
            // consonants. 
            if (src[offset] == '(' || src[offset] == ')')
            {
                var charToInsert = src[offset] switch
                {
                    '(' => "\u035c", // COMBINING DOUBLE BREVE BELOW (U+035C)
                    ')' => "\u0361", // COMBINING DOUBLE INVERTED BREVE (U+0361)
                    _ => throw new InvalidOperationException()
                };

                // We "spool back" in the output buffer
                // until we hit the first character that
                // is not a combining mark or zero.
                var insertionOffset = output.Length - 1;
                while (insertionOffset > 0)
                {
                    if (CharUnicodeInfo.GetUnicodeCategory(output[insertionOffset]) != UnicodeCategory.NonSpacingMark)
                        break;
                    insertionOffset--;
                }
                if (insertionOffset < 0)
                {
                    insertionOffset = 0;
                }
                output.Insert(insertionOffset, charToInsert);

                // Now that we've inserted the combining character,
                // we advance the outer loop to the next character.
                offset++;
                continue;
            }

            // We first select the subset of characters which
            // start with the character at offset.
            var xsSubset = from ch in Characters
                           where selector(ch).StartsWith(src[offset])
                           select ch;
            // NOTE: the selection is by the documentation
            // performed case-sensitively and with an invariant
            // culture.

            // In the case that we have no matches at the
            // current offset, we simply push the current
            // character to the builder and advance by one.
            int matchCount = xsSubset.Count();
            if (matchCount < 1)
            {
                output.Append(src[offset]);
                offset++;
                continue;
            }
            // If, however, there is a single match,
            // we just consume it right away.
            else if (matchCount == 1)
            {
                var result = xsSubset.Single().Ipa;
                output.Append(result);
                offset++;
                continue;
            }

            // If we get here, we have more than one possible
            // match. So we will try to find the longest and
            // then look for shorter sequences until we get
            // a single match.

            string strToInsert = null;

            // THe longest possible match.
            var longestMatch = xsSubset.Max(x => selector(x).Length);

            // With need to clamp the previous value in case we're near the edge.
            longestMatch = Math.Min(
                longestMatch,
                src.Length - offset
                );

            while (longestMatch > 0)
            {
                var substr = src.Substring(offset, longestMatch);
                var matches = from ch in xsSubset
                              where selector(ch) == substr
                              select ch;

                var count = matches.Count();

                // If we have one match, we push the IPA
                // into the output buffer and advance the
                // offset.
                if (count == 1)
                {
                    strToInsert = matches.Single().Ipa;

                    // We move the offset by the length
                    // of the matched substring.
                    offset += substr.Length;
                    break;
                }

                // We decrement
                longestMatch--;
            }
            output.Append(strToInsert);

            // If we get this far in far, something
            // has gone wrong or something.
            // This code should logically be really
            // unreachable.
            // // -----------------------------------------
            // // So just in case, we push the current
            // // character to the outout buffer and
            // // advance the offset.
        }

        // Finally, we return the built string.
        return output.ToString();
    }

    public static string? ParseXSampa(this string? xsampa, Action<XSampaOptions>? options)
    {
        var opts = new XSampaOptions();
        options?.Invoke(opts);

        return ParseXSampa(xsampa, opts);
    }

    public static readonly XSampaOptions DefaultOptions = new();

    private record XSampaCharacter
    (
        string XSampa,
        string Ipa,
        string? ZSampa = null,
        string? Cxs = null,

        bool Ascender = false,
        bool Descender = false,

        bool Combining = false,
        AccentKind Accent = AccentKind.NotAnAccent,
        string? Opposite = null
    );

    private enum AccentKind
    {
        NotAnAccent = 0,
        Below = -1,
        Above = 1
    }

    private static readonly IEnumerable<XSampaCharacter> Characters;

    static XSampa()
    {
        Characters = LoadPairs();
    }

    private static IEnumerable<XSampaCharacter> LoadPairs()
    {
        // First we load the resource.
        var asembly = typeof(XSampa).Assembly;
        using var stream = asembly.GetManifestResourceStream("Minustar.Website.phonetic.json");
        var document = JsonDocument.Parse(stream);

        var list = new List<XSampaCharacter>();

        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            var arrayEnum = document.RootElement.EnumerateArray();
            foreach (var it in arrayEnum)
            {
                var ipa = it.GetProperty("ipa").GetString();
                var xsampa = it.GetProperty("xsampa").GetString();

                var ccls = it.GetProperty("ccls").GetInt32();
                var accent = ccls switch
                {
                    230 => AccentKind.Above,
                    220 => AccentKind.Below,
                    _ => AccentKind.NotAnAccent
                };

                list.Add(new XSampaCharacter(
                    xsampa,
                    ipa,
                    Combining: ccls > 0,
                    Accent: accent
                    ));
            }
        }

        return list;
    }
}
