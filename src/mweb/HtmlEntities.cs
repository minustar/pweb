namespace Minustar.Website;

public static class HtmlEntities
{
    private static Dictionary<string, HtmlEntity>? entities;

    public static HtmlEntity? Find(string? name)
    {
        if (name is null)
            return null;

        LoadEntities();

        if (entities?.TryGetValue(name, out var entity) == true)
            return entity;

        return null;
    }

    public static string? GetCharacter(string? name)
    {
        var ent = Find(name);
        if (ent is not null)
            return ent.Characters;

        return name;
    }

    private static void LoadEntities()
    {
        if (entities?.Count > 0)
            return;

        using var stream = typeof(HtmlEntities).Assembly
                            .GetManifestResourceStream("Minustar.Website.entities.json");
        if (stream is null)
            return;

        entities = new Dictionary<string, HtmlEntity>();

        int ccnt = 0;
        using var doc = JsonDocument.Parse(stream);
        foreach (var property in doc.RootElement.EnumerateObject())
        {
            ccnt++;
            var key = property.Name[1..];
            if (key.EndsWith(";"))
                key = key[..^1];

            // Does the list already include this key?
            if (entities.ContainsKey(key))
                continue;

            Debug.WriteLine("{0,-6}: {1}", ccnt, property.Name);

            var value = property.Value;
            try
            {
                var cpArrayElem = property.Value.GetProperty("codepoints");
                int[] codePoints = new int[cpArrayElem.GetArrayLength()];
                int i = 0;
                foreach (var elem in cpArrayElem.EnumerateArray())
                    codePoints[i++] = elem.GetInt32();

                var chsArrayElem = property.Value.GetProperty("characters").GetString();
                //string?[] characters = new string?[chsArrayElem.GetArrayLength()];
                //i = 0;
                //foreach (var elem in chsArrayElem.EnumerateArray())
                //    characters[i++] = elem.GetString();

                var ent = new HtmlEntity(key, codePoints, chsArrayElem);
                entities.Add(key, ent);
            }catch (Exception ex)
            {
                Debug.WriteLine($"Failed to add {{{property.Name}}}");
                Debug.WriteLine(ex);
            }
        }
    }

    public record HtmlEntity(
        string Key,
        IEnumerable<int>? CodePoints, 
        string? Characters
        );
}