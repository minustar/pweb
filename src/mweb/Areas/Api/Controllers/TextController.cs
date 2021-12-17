namespace Minustar.Website.Areas.Api.Controllers;

[Area("API")]
[ApiController]
[Route("api/[controller]")]
public class TextController : UtilityConttoller
{
    [Route("convert")]
    [HttpGet]
    [HttpPost]
    public async Task<string?> COnvertToIpa(string? text = null)
    {
        if (text is null && HttpContext.Request.Method == "POST")
            text = await ReadBodyAsync();

        try
        {
            var result = Convert(text);
            return result;
        }
        catch
        {
            return text;
        }

    }

    private static string? Convert(string? text)
    {
        if (text is null)
            return null;

        var b = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\\')
            {
                if (i == text.Length - 1)
                {
                    b.Append(c);
                }
                else if (TryConsumeEntity(text,
                                             ref i,
                                             out string? @char))
                {
                    b.Append(@char);
                }
                else
                {
                    b.Append(text[++i]);
                }
            }
            else
            {
                b.Append(c);
            }
        }

        return b.ToString();
    }

    private static bool TryConsumeEntity(string? text, ref int i, out string? @char )
    {
        @char = null;

        if (text is null)
            return false;

        i++;
        char c = text[i];
        if (c != '(')
            return false;

        i++;
        var b = new StringBuilder();
        while (i < text.Length && ((c = text[i++]) != ')'))
        {
            b.Append(c);
        }

        string selectedEntity = b.ToString();
        if (int.TryParse(selectedEntity, HexNumber, InvariantCulture, out int uccode))
        {
            @char = Char.ConvertFromUtf32(uccode);
            i--;
            return true;
        }

        var entity = HtmlEntities.Find(selectedEntity);
        if (entity is not null)
        {
            @char = entity.Characters;
            i--;
            return true;
        }

        return false;
    }
}