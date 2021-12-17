using Microsoft.AspNetCore.Html;

namespace Minustar.Website.QSyntax;

public partial class QSyntaxParser
{
    /**
     * Syntax:
     * \(name) - named entity
     * \(#xx.xx.xx) - series of unicode characters.
     * 
     * \macro
     * \macro{mandatory}
     * \macro[opt]{mandatory}
     * {selector (options) text}
     * {[text]]
     * [[link][text]]
     **/

    private static readonly QSyntaxParser i;

    private readonly Dictionary<string , HtmlNamedEntity> entities;

    static QSyntaxParser() => i = new QSyntaxParser();
    private QSyntaxParser()
    {
        entities = new ();
    }

    public static QSyntaxParser Instance => i;

    public static IHtmlContent ParseQSyntax(
        string source,
        TagBuilder? wrapInTag = null) => Instance.Parse(source, wrapInTag);  

    public IHtmlContent Parse(
        string source,
        TagBuilder? wrapInTag = null)
    {
        // TODO: Temporarily, just refer to the old method.
        return _QSyntaxParser.ParseQSyntax(source);

        IHtmlContent contents = wrapInTag?.InnerHtml ?? new HtmlContentBuilder();
        bool escaped = false;
        var accumulator = new StringBuilder();

        // We will consume the character stream one
        // character at a time.
        // We will, however, meed to track the current
        // offset.
        int offset = 0;
        while (offset < source.Length)
        {
            char currChar = source[offset];

            if (escaped)
            {
                // Macro?
                // Entity?
                // Special char?
            }
            else if (currChar == '\\')
            {
                escaped = true;
                continue;
            }
            else if (currChar == '{')
            {
                // This represents a sequence.
                // Like "{na native}".
                int originalOffset = offset;
                var success = TryParseSequence(source, ref offset, out InlineSequence sequence);
                if (success)
                {
                    RenderSequence(contents, sequence);
                }
                else
                {
                    throw new FormatException($"A sequence couldn't be parsed starting at offset {orignalOffset}.");
                }
            }
        }

        return wrapInTag ?? contents;
    }

    private bool TryParseSequence(string source, ref int offset, out InlineSequence sequence)
    {
        bool escaped = false;

        char c;
        var acc = new StringBuilder();
        for (; offset < source.Length; offset++)
        {
            c = source[offset];

            if (escaped)
            {
                acc.Append(c);
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
            }
            else if (c == '}')
            {
                break;
            }
            else
            {
                acc.Append(c);
            }
        }

        bool star = false;
        var selector = new StringBuilder();
        var switchOpt = new StringBuilder();
        var options = new StringBuilder();
        var text = new StringBuilder();

        int i = 0;
        for (i = 0; char.IsLetter(c = acc[i]) && i < acc.Length; i++)
            selector.Append(c);

        if (c == ':')
            for (i++; char.IsLetter(c = acc[i]) && i < acc.Length; i++)
                switchOpt.Append(c);

        if (c == '*')
            star = true;

        // we consume all whitespace.

        if (c == '(')
        {
            for (i++; (c = acc[i]) != ')' && i < acc.Length; i++)
                options.Append(c);
            for (; char.IsWhiteSpace(c = acc[i]) && i < acc.Length; i++) ;
        }

        text.Append(acc.ToString()[i..]);

        sequence = new InlineSequence(
            selector.ToString(),
            switchOpt.Length > 0 ? switchOpt.ToString() : null,
            star,
            options.Length > 0 ? options.ToString() : null,
            text.ToString()
            );
        return true;
    }

    public record InlineSequence (
        string Selector, 
        string? Switch,
        bool Star,
        string? Options,
        string Text
        );
}

public class _QSyntaxParser
{
    public static IHtmlContent ParseQSyntax(
        string source,
        Action<QSyntaxParserOptions>? options = null)
    {
        var opts = new QSyntaxParserOptions();
        options?.Invoke(opts);

        var content = new HtmlContentBuilder();
        bool escaped = false;
        var acc = new StringBuilder();
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (escaped)
            {
                acc.Append(c);
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
            }
            else if (c == '~')
            {
                acc?.Append(NonbreakingSpace);
            }
            else if (c == '{')
            {
                if (acc?.Length > 0)
                {
                    PushStringBufferToContents(content, acc);
                }
                // Move advance by one
                i++;
                content.AppendHtml(ProcessTag(source, ref i));
            }
            else
            {
                acc?.Append(c);
            }
        }
        if (acc?.Length > 0)
        {
            PushStringBufferToContents(content, acc);
        }

        if (opts?.OuterTag is not null)
        {
            var tag = new TagBuilder(opts.OuterTag);
            tag.InnerHtml.SetHtmlContent(content);

            return tag;
        }

        return content;
    }

    private static IHtmlContent? ProcessTag(string source, ref int i)
    {
        var sb = new StringBuilder();

        bool escaped = false;
        for (; i < source.Length; i++)
        {
            char c = source[i];
            if (escaped)
            {
                sb.Append(c);
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
            }
            else if (c == '}')
            {
                break;
            }
            else
            {
                sb.Append(c);
            }
        }

        return ProcessTagContents(sb.ToString());
    }

    private static IHtmlContent ProcessTagContents(string str)
    {
        string selector = string.Empty;
        string options = string.Empty;
        string text = string.Empty;

        char c = '\0';
        int i = 0;

        // Let's consume until the first whitespace.
        for (; i < str.Length && !char.IsWhiteSpace((c = str[i])); i++)
            selector += c;

        // Consume whitespace.
        for (; i < str.Length && char.IsWhiteSpace(c = str[i]); i++)
            /* Do nothing. */
            ;

        if (c == '(')
        {
            // Consume options.
            i++;
            for (; i < str.Length && (c = str[i]) != ')'; i++)
                options += c;
            i++;

            // Consume whitespace.
            for (; i < str.Length && char.IsWhiteSpace(c = str[i]); i++)
                /* Do nothing. */
                ;
        }

        for (; i < str.Length; i++)
            text += str[i];

        if (selector == "q")
        {
            var tag = new TagBuilder("q");
            tag.InnerHtml.Append(text);

            return tag;
        }

        bool isStarred = false;
        if (selector.EndsWith("*"))
        {
            isStarred = true;
            selector = selector[0..^1];
        }

        string? @switch = null;
        if ((i = selector.IndexOf(':')) != -1)
        {
            @switch = selector[(i + 1)..];
            selector = selector[..i];
        }

        return selector switch
        {
            "na" or "n" or "native" => BuildTag("native", isStarred, text),
            "fo" or "f" or "native" => BuildTag("foreign", isStarred, text),
            "ph" or "phoneme" => BuildPhoneticTag("phoneme", isStarred, text, @switch == "xs"),
            "al" or "allophone" => BuildPhoneticTag("allophone", isStarred, text, @switch == "xs"),
            "er" or "eroot" => BuildTag("etym-root", isStarred, text),
            "el" or "elang" => BuildTag("etym-lang", isStarred, text),
            "c" or "cs" or "case" => BuildTag("case", isStarred, text),
            _ => new HtmlString(text)
        };
    }

    private static IHtmlContent BuildPhoneticTag(string cssClass, bool star, string text, bool parseAsXSampa)
    {
        var tag = new TagBuilder("span");
        tag.AddCssClass("phonetic");
        tag.AddCssClass(cssClass);

        if (star)
        {
            tag.AddCssClass("err-star");
        }
        if (parseAsXSampa)
        {
            tag.InnerHtml.Append(text.ParseXSampa());
        }
        else
        {
            tag.InnerHtml.Append(text);
        }

        return tag;
    }

    private static IHtmlContent BuildTag(string cssClass, bool star, string text)
    {
        var tag = new TagBuilder("span");
        tag.AddCssClass(cssClass);
        if (star)
            tag.AddCssClass("err-star");
        tag.InnerHtml.Append(text);

        return tag;
    }

    private static void PushStringBufferToContents(HtmlContentBuilder content, StringBuilder? acc)
    {
        var str = acc?.ToString() ?? string.Empty;
        var html = new HtmlString(str);

        content.AppendHtml(html);
        acc?.Clear();
    }
}
