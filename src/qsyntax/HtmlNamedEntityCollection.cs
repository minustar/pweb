namespace Minustar.Website.QSyntax;

/// <summary>
/// Represents a collection of the known
/// named HTML entities, as defined by the
/// W3C in the draft for HTMl5.
/// </summary>
public class HtmlNamedEntityCollection
    : IReadOnlyCollection<HtmlNamedEntity>,
      IReadOnlyDictionary<String, HtmlNamedEntity>
{
    // Internal storage.
    private readonly List<HtmlNamedEntity> internalStore;

    // The soliton instance.
    private static readonly HtmlNamedEntityCollection instance;

    // Shorthand
    private readonly Comparison<string> strComp = CultureInfo
        .InvariantCulture
        .CompareInfo
        .Compare;

    // Static constructor that initializes the
    // soliton instance.
    static HtmlNamedEntityCollection() => instance = new();

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    public HtmlNamedEntity this[int index]
    {
        get => internalStore[index];
    }

    /// <summary>
    /// Gets the soliton instance.
    /// </summary>
    public static HtmlNamedEntityCollection Instance => instance;

    /// <summary>
    /// Gets the element with the specified name.
    /// </summary>
    public HtmlNamedEntity this[string name]
    {
        get
        {
            // We iterate through the internal
            // store list until we find a matching
            // key.
            foreach (var item in internalStore)
                if (strComp(item.Name, name) == 0)
                    return item;

            // We might not have found any matching
            // entity.
            throw new KeyNotFoundException();
        }
    }

    /// <summary>
    /// Gets an enumerable collection that contains
    /// the keys in the read-only dictionary.
    /// </summary>
    public IEnumerable<String> Keys
    {
        get
        {
            foreach (var entity in internalStore)
                yield return entity.Name;
        }
    }

    /// <summary>
    /// Gets an enumerable collection that contains
    /// the values in the read-only dictionary.
    /// </summary>
    public IEnumerable<HtmlNamedEntity> Values
    {
        get
        {
            foreach (var entity in internalStore)
                yield return entity;
        }
    }

    /// <summary>
    /// Creates a new instance of the class
    /// <see cref="HtmlNamedEntityCollection"/>.
    /// </summary>
    private HtmlNamedEntityCollection()
    {
        internalStore = new List<HtmlNamedEntity>();
        LoadKnownEntities();
    }

    private void LoadKnownEntities()
    {
        const string RES_NAME = "Minustar.Website.entities.json";

        // Let's get the stream from the assembly manifest.
        // Should this fail, we throw an exception.
        var assembly = typeof(HtmlNamedEntity).Assembly;
        using var stream = assembly.GetManifestResourceStream(RES_NAME);
        if (stream is null)
            throw new FileNotFoundException(
                message: "The embedded resource file 'entities.json' couldn't be found.",
                fileName: RES_NAME
            );

        // We will now attempt to get a JSON document from
        // the resource file.
        // If this throws any exceptions, we should let that
        // bubble up in the system.
        var jsonDoc = JsonDocument.Parse(stream);

        // The root element that we will iterate over.
        var root = jsonDoc.RootElement;
        foreach (var property in root.EnumerateObject())
            ProcessEntity(property);
    }

    /// <summary>
    /// Processes a <see cref="JsonProperty"/> as
    /// a representation of a known named HTML entity.
    /// </summary>
    /// <param name="property">
    /// The property to process.
    /// </param>
    private void ProcessEntity(JsonProperty property)
    {
        // We will retrieve the name of the property,
        // which corresponds to the key for that item.
        // We will also "sanitize" the entity, as for
        // some obscure reason, the entity resource file
        // we use might have duplicates like '&AElig;'
        // and '&AElig'.
        var name = property.Name;
        name = SanitizeJsonPropertyName(name);

        // If the entity list already contains the name,
        // we skip it.
        if (CheckEntityNameAlreadyKnown(name))
            return;

        // Each item in the resource file should have
        // two properties: codepoints, which should be an
        // array of int32, and characters which should be
        // a string.
        // Should these fail, we terminate the processing´
        // here using an exception that will bubble up.
        var value = property.Value;
        var codepoints = TryGetArrayOfInt32(value.GetProperty("codepoints"), out int[] intArray)
            ? intArray
            : throw new FormatException("An array of int32 values in JSON was excepted.");
        var characters = value.GetProperty("characters").GetString()
            ?? throw new FormatException("A string was expected.");

        // We're got all the parts, so we create a new
        // instance and add it to the list.
        var entity = new HtmlNamedEntity(
            name,
            codepoints,
            characters
            );
        internalStore.Add(entity);
    }

    /// <summary>
    /// Tries to retrive an array of int32 from a JSON
    /// element.
    /// </summary>
    /// <param name="elem">
    /// The JSOn element that should be attempted converted.
    /// </param>
    /// <param name="intArray">
    /// The array of int32 retrieved from the conversion.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the conversion was
    /// successful; otherise <see langword="false"/>:
    /// </returns>
    private bool TryGetArrayOfInt32(JsonElement elem, out int[] intArray)
    {
        // Setting a default value.
        intArray = new int[0];

        // If the value isn't an array, we don't need
        // to continue, and fail the call.
        if (elem.ValueKind != JsonValueKind.Array)
            return false;

        // Initializing the array, and will then
        // iterate over the elements fo the JSON array
        // itself, trying to convert them to int32
        // values.
        intArray = new int[elem.GetArrayLength()];
        int index = 0;
        foreach (var item in elem.EnumerateArray())
            if (!item.TryGetInt32(out intArray[index++]))
                // If the value can't be parsed as an int32,
                // we terminate and fail the method.
                return false;

        // Getting here means that the contents of the
        // property have successfully been parsed to
        // an array of int32.
        return true;
    }

    /// <summary>
    /// Returns a value indicated whether an entity
    /// name is already present in the internal list.
    /// </summary>
    /// <param name="name">
    /// The name to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="name"/>
    /// was found in the internal list; 
    /// otherise <see langword="false"/>.
    /// </returns>
    private bool CheckEntityNameAlreadyKnown(string name)
    {
        // We iterate in the reverse order, as it
        // is most likely that the keys provided
        // in the entity resource file are already
        // in alphabetical order, which that the last
        // added entity to the internal storage list
        // is the closest to the current name that
        // we have to check.
        for (int index = internalStore.Count - 1; index >= 0; index--)
            if (strComp(internalStore[index].Name, name) == 0)
                return true;

        // If we get here, we haven't found a matching name.
        return false;
    }

    /// <summary>
    /// Returns a sanitized entity name.
    /// </summary>
    /// <param name="str">
    /// The <see cref="string"/> to sanitize.
    /// </param>
    /// <returns>
    /// A sanitized entity name.
    /// </returns>
    private string SanitizeJsonPropertyName(string str)
    {
        // This is self-explanatory.
        if (!str.StartsWith('&'))
            throw new FormatException("All entity keys in the resource file should start with the character '&'.");

        // If the name ends with a semi-colon, we return
        // the substring from the second to the second to
        // last character in the original string; otherwise
        // we just return the substring starting from the
        // second character.
        if (str.EndsWith(';'))
            return str[1..^1];
        return str[1..];
    }

    /// <summary>
    /// Gets the entity associated with a specific name.
    /// </summary>
    /// <param name="name">
    /// The name of the entity to retrieve.
    /// </param>
    /// <param name="value">
    /// When this method returns, the value associated with
    /// the specified name, if the key is found; otherwise, 
    /// <see langword="null"/>. 
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the entity associated with
    /// <paramref name="name"/> was found; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGetValue(string name, [MaybeNullWhen(false)] out HtmlNamedEntity value)
    {
        // We iterate over the elements in the internal
        // list, and should one match the name, we 
        // assign the matching entity to the value argument
        // and return true.
        for (int index = 0; index < internalStore.Count; index++)
            if (strComp(internalStore[index].Name, name) == 0)
            {
                value = internalStore[index];
                return true;
            }

        // If no value has matched so far, we return
        // false.
        value = default;
        return false;
    }

    /// <summary>
    /// Gets the number of known entities.
    /// </summary>
    public int Count => internalStore.Count;

    /// <summary>
    /// Returns an enumerator that iterates through 
    /// the collection.
    /// </summary>
    /// <returns>
    /// An enumerator that can be used to iterate
    /// through the collection.
    /// </returns>
    public IEnumerator<HtmlNamedEntity> GetEnumerator()
    {
        return ((IEnumerable<HtmlNamedEntity>)internalStore).GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through
    /// a collection.
    /// </summary>
    /// <returns>
    /// An <see cref=">IEnumerator"/> object that can
    /// be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)internalStore).GetEnumerator();
    }

    /// <summary>
    /// Determines whether an entity with the specified
    /// name exists.
    /// </summary>
    /// <param name="name">
    /// The name to lookup.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the entity associated with
    /// <paramref name="name"/> was found; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool ContainsKey(string name)
    {
        // We iterate through all the items in the internal
        // list, and return true, if any matches thedefired
        // key.
        for (int index = 0; index < internalStore.Count; index++)
            if (strComp(internalStore[index].Name, name) == 0)
                return true;

        // The key was never matched.
        return false;
    }

    IEnumerator<KeyValuePair<string, HtmlNamedEntity>> IEnumerable<KeyValuePair<string, HtmlNamedEntity>>.GetEnumerator()
    {
        return new KeyValuePairEnumerator(this);
    }

    /// <summary>
    /// An iterator through the known named entities.
    /// </summary>
    private struct KeyValuePairEnumerator : IEnumerator<KeyValuePair<string, HtmlNamedEntity>>
    {
        private int currentIndex = 0;
        private readonly HtmlNamedEntityCollection entityCollection;

        /// <summary>
        /// Creates a new instance of the class
        /// <see cref="KeyValuePairEnumerator"/> for
        /// the specified 
        /// <see cref="HtmlNamedEntityCollection"/>.
        /// </summary>
        /// <param name="entityCollection">
        /// A <see cref="HtmlNamedEntityCollection"/>.
        /// </param>
        public KeyValuePairEnumerator(HtmlNamedEntityCollection entityCollection)
        {
            this.entityCollection = entityCollection;
        }

        /// <summary>
        /// Gets the element in the collection at the
        /// current position of the enumerator.
        /// </summary>
        public KeyValuePair<string, HtmlNamedEntity> Current
            => entityCollection[currentIndex];

        /// <summary>
        /// Gets the element in the collection at the
        /// current position of the enumerator.
        /// </summary>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Performs application-defined tasks 
        /// associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Does nothing, as there is nothing to
            // dispose.
        }

        /// <summary>
        /// Advances the enumerator to the next element
        /// of the collection.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the enumerator was
        /// successfully advanced to the next element;
        /// <see langword="false"/> if the enumerator has
        /// passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            var newIndex = currentIndex + 1;
            if (newIndex >= entityCollection.Count)
                return false;

            currentIndex = newIndex;
            return true;
        }

        /// <summary>
        /// Sets the enumerator to its initial position,
        /// which is before the first element in the
        /// collection.
        /// </summary>
        public void Reset()
        {
            currentIndex = 0;
        }
    }
}

/// <summary>
/// an HTML named entity..
/// </summary>
/// <param name="Name">
/// The name of the entity, that is 
/// <code>&amp;&lt;name&gt;;</code>.
/// </param>
/// <param name="CodePoints">
/// The codepoints associated with this entity.
/// </param>
/// <param name="Characters">
/// The <see cref="string"/> representation of this entity.
/// </param>
public record HtmlNamedEntity(
    string Name,
    IEnumerable<int> CodePoints,
    string Characters
) : IComparable<HtmlNamedEntity>
{
    /// <summary>
    /// Compare this instance with another.
    /// </summary>
    /// <param name="other">
    /// Another instance.
    /// </param>
    /// <returns>
    /// An <see creF="int"/> specifying the order.
    /// </returns>
    public int CompareTo(HtmlNamedEntity? other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));

        return CultureInfo.InvariantCulture
            .CompareInfo
            .Compare(
                this.Name,
                other.Name
            );
    }

    /// <summary>
    /// Implicitly converts an instance of
    /// <see creF="HtmlNamedEntity"/> to its
    /// <see cref="P:Characters"*>.
    /// </summary>
    /// <param name="entity">
    /// The value to convert.
    /// </param>
    /// <returns>
    /// A <see cref="string"/>.
    /// </returns>
    public static implicit operator string(HtmlNamedEntity entity)
        => entity.Characters;

    /// <summary>
    /// Implicitly converts an instance of
    /// <see cref="HtmlNamedEntity"/> to a
    /// <see creF="KeyValuePair{K, V}"/>.
    /// </summary>
    /// <param name="ent">
    /// The entity to convert.
    /// </param>
    /// <returns>
    /// A <see creF="KeyValuePair{K, V}"/>.
    /// </returns>
    public static implicit operator KeyValuePair<string, HtmlNamedEntity>(HtmlNamedEntity ent)
        => new(ent.Name, ent);
}