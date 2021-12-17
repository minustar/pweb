namespace Minustar.Website.QSyntax;

partial class QSyntaxParser
{
    /// <summary>
    /// Tries to parse a numeric entity.
    /// </summary>
    /// <param name="str">
    /// The string to parse.
    /// </param>
    /// <param name="value">
    /// If successful, the string of characters
    /// that were parsed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the parsing of
    /// <paramref name="str"/> succeeded;
    /// otherwise <see langword="false"/>.
    /// </returns>
    private bool TryParseNumericEntity(string str, out string? value)
    {
        // Setting out value to the default.
        value = null;

        // The contents that we will parse consist of
        // a series of hex digits separated by
        // one of the following [ ,.].
        var tokens = str.Split(new char[] { '.', ',', ' ' });

        // We will now iterate over the tokens.
        var builder = new StringBuilder();
        foreach (var token in tokens)
        {
            // If we get false as the result, the
            // parsingf of the token failed, therefore
            // we need to stop the parsing.
            if (!ProcessNumericEntityToken(builder, token))
                return false;
        }

        // Parsing the tokens succeeded.
        value = builder.ToString();
        return true;
    }

    /// <summary>
    /// Processes a token from a numeric entity.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="StringBuilder"/> where the
    /// characters get accumulated.
    /// </param>
    /// <param name="token">
    /// The token to parse.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the parsing of
    /// <paramref name="token"/> succeeded;
    /// otherwise <see langword="false"/>.
    /// </returns>
    private bool ProcessNumericEntityToken(StringBuilder builder, string token)
    {
        // Let's check if the token is matching
        // a hex number, if not, we kill the processing
        // here.
        if (!Int32.TryParse(token, HexNumber, InvariantCulture, out var codePoint))
            return false;

        // The number was a valid Hex number, so
        // we convert the code point to a string.
        // Note this lets use use hex numbers that
        // are beyond 16-bit integers, like characters
        // in the BMP.
        try
        {
            var ch = Char.ConvertFromUtf32(codePoint);
            builder.Append(ch);
        }
        catch (ArgumentOutOfRangeException)
        {
            // According to the documentation,'
            // ConvertFromUtf32(...) can throw
            // this if the number doesn't match
            // a 21-bit integer as per the Unicode
            // requirements.
            return false;
        }

        return true;
    }

    /// <summary>
    /// Looks up an entity by name from the loaded
    /// entities.
    /// </summary>
    /// <param name="name">
    /// The name to look up.
    /// </param>
    /// <param name="value">
    /// The value, if found, of the entity.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, if the entity is found;
    /// otherwise <see langword="false"/>.
    /// </returns>
    private bool TryLookupNamedEntity(string name, out string? value)
    {
        // Setting the default value.
        value = null;

        // Let's make sure all the entities are loaded.
        VerifyNamedEntitiesAreLoaded();

        // Trying to get the value from the dictionary
        // of known entities.
        if (entities.TryGetValue(name, out HtmlNamedEntity? ent))
        {
            value = ent.Characters;
            return true;
        }

        // Finally, if we get here, we haven't found the entity.
        return false;
    }

    /// <summary>
    /// Loads the entities, if they aren't loaded
    /// already.
    /// </summary>
    private void VerifyNamedEntitiesAreLoaded()
    {
        const string RESOURCE_NAME = "Minustar.Website.entities.json";

        // Let's check the entities are loaded.
        if (entities.Count > 0)
            return;

        // We are going to retrieve the names of
        // the entities from an embedded resource
        // file.
        var assembly = typeof(QSyntaxParser).Assembly;
        using var resStream = assembly.GetManifestResourceStream(RESOURCE_NAME);

        // IF the stream is null, something is really
        // wrong.
        if (resStream is null)
            throw new FileNotFoundException(RESOURCE_NAME);

        // It's a JSON file, so we will load it as
        // a JsonDociument
        using var jsonDoc = JsonDocument.Parse(resStream);
        var root = jsonDoc.RootElement;

        // Now, we will iterator for the enumerated
        // members of the root element.
        foreach (var prop in root.EnumerateObject())
        {
            ProcessEntityProperty(prop);
        }
    }

    /// <summary>
    /// Retrieves an <see cref="HtmlNamedEntity"/> from
    /// a <see cref="JsonProperty"/>.
    /// </summary>
    /// <param name="prop">
    /// The <see cref="JsonProperty"/>.
    /// </param>
    private void ProcessEntityProperty(JsonProperty prop)
    {
        // For some reason, the JSON source
        // document, there are duplicates.
        // For example, the &AElig; entity is
        // also listed as &AELig. What we will
        // do now, is "sanitize the entity name.
        var name = SanitizeLoadedEntityName(prop.Name);

        // If the entity dictionary already
        // contains this name, we skip it.
        if (entities.ContainsKey(name))
            return;

        // Each item in the source JSON file are
        // presented with this format (here, "prettyief").
        //   "&AMP": {
        //     "codepoints": [38],
        //     "characters": "\u0026"
        //   }
        // In short, two properties that must be ther.
        if (!prop.Value.TryGetProperty("codepoints", out var cpProp))
            throw new InvalidOperationException("The property 'codepoints' couldn't be found.");
        if (!prop.Value.TryGetProperty("characters", out var chProp))
            throw new InvalidOperationException("The property 'characters' couldn't be found.");

        // From the above presented format, we know that
        // the property 'codepoints' is an array of int32
        // values. So we now, "unroll" the array.
        var codepoints = ReadJsonPropertyAsArrayOfIn32s(cpProp);
        var characters = chProp.GetString();

        // We create the instance, and
        // add it to the dictionary.
        var entity = new HtmlNamedEntity(name, codepoints, characters);
        entities.Add(name, entity);
    }

    /// <summary>
    /// Reads the contents of a <see cref="JsonElement"/> as
    /// an array of <see cref="int"/> values.
    /// </summary>
    /// <param name="elem">
    /// The JSON element to convert.
    /// </param>
    /// <returns>
    /// An array of <see cref="int"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// If <paramref name="elem"/> is no a <see cref="JsonValueKind.Array"/>;
    /// or if one of the elements in the array isn't an <see cref="int"/>.
    /// </exception>
    private int[] ReadJsonPropertyAsArrayOfIn32s(JsonElement elem)
    {
        // The element must be an array
        if (elem.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("The element must be an array.");

        // We will enumerate over the array.
        int length = elem.GetArrayLength();
        var array = new int[length];
        int index = 0;
        foreach (var item in elem.EnumerateArray())
        {
            if (!item.TryGetInt32(out var i))
                throw new InvalidOperationException("The values of the array must be int32.");

            array[index++] = i;
        }

        return array;
    }

    /// <summary>
    /// Sanitizes a name.
    /// </summary>
    /// <param name="name">
    /// The name to sanitize.
    /// </param>
    /// <returns>
    /// The sanitized name.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// If the name doesn't start with '&amp;'.
    /// </exception>
    private string SanitizeLoadedEntityName(string name)
    {
        if (!name.StartsWith("&"))
            throw new InvalidOperationException("The entity name should start with '&'.");

        if (name.EndsWith(";"))
            return name[1..^1];

        return name[1..];
    }

    /// <summary>
    /// Represents an HTML named entity.
    /// </summary>
    /// <param name="Name">
    /// The name of the entity.
    /// </param>
    /// <param name="CodePoints">
    /// An array of <see cref="int"/> representing the
    /// codepoints associated with this entity.
    /// </param>
    /// <param name="Characters">
    /// The <see cref="string"/> representation of
    /// this entity.
    /// </param>
    public record HtmlNamedEntity(string Name, int[] CodePoints, string Characters);
}