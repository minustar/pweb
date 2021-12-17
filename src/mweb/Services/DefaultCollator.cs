namespace Minustar.Website.Services;

public sealed class DefaultCollator : ICollator
{
    private static readonly ICollator me;
    private readonly ICollator implementation;

    public static ICollator Instance => me;

    #region Constructors

    static DefaultCollator() => me = new DefaultCollator();

    private DefaultCollator() => implementation = new DefaultCollatorImplementation();

    #endregion

    public LetterInfo? GetFirstLetter(string? str) => implementation.GetFirstLetter(str);
    public IEnumerable<LetterInfo>? GetLetters(string? str) => implementation.GetLetters(str);
    public int Compare(ISortable? x, ISortable? y) => implementation.Compare(x, y);
    public int Compare(string? x, string? y) => implementation.Compare(x, y);

    private class DefaultCollatorImplementation : ICollator
    {
        public LetterInfo? GetFirstLetter(string? str)
        {
            return GetLetters(str)?.FirstOrDefault();
        }

        public IEnumerable<LetterInfo> GetLetters(string? str)
        {
            if (str is null || str.Length == 0)
                yield break;

            var norm = str.Normalize(NormalizationForm.FormC);
            var queue = new Queue<char>(norm);

            int baseOffset = 0x1000;
            while (queue.Count > 0)
            {
                char currentChar = queue.Dequeue();
                string currentStr = new string(currentChar, 1)
                    .Normalize(NormalizationForm.FormD);

                int primary = Char.ToUpperInvariant( currentStr[0]);
                short secondary = (short)(char.IsLower(currentStr[0]) ? 1 : 0);
                sbyte ter = 0x00, quad = 0x00;

                if (currentStr.Length > 1)
                {
                    ter  = (sbyte)(40 + ((int)(currentStr[1]) & 0x00FF));
                    quad = (sbyte)(10 + (((int)(currentStr[1]) & 0xFF00) >> 8));
                }

                yield return new LetterInfo(currentStr,
                                            primary + baseOffset,
                                            secondary,
                                            ter,
                                            quad);
            }
        }

        //public IEnumerable<LetterInfo>? GetLettesAlt(string? str)
        //{
        //    if (str is null || str.Length == 0)
        //        yield break;

        //    var norm = str.Normalize(NormalizationForm.FormD);
        //    for (int i = 0; i < norm.Length; i++)
        //    {
        //        var ch = norm.Substring(i, 1).Normalize(NormalizationForm.FormC);

        //        int primary = (int)Char.ToUpperInvariant(ch[0]);
        //        short secondary = ch.Length > 1
        //            ? (short)ch[1]
        //            : (short)0;
        //        sbyte tertiary = (sbyte)(Char.IsUpper(ch[0]) ? 0 : 1);

        //        yield return new LetterInfo(ch,
        //                                    primary,
        //                                    secondary,
        //                                    tertiary);
        //    }
        //}

        public int Compare(ISortable? x, ISortable? y)
        {
            string? keyX = x?.SortKey ?? x?.Headword;
            string? keyY = y?.SortKey ?? y?.Headword;

            return Compare(keyX, keyY);
        }

        public int Compare(string? x, string? y)
        {
            var lettersX = GetLetters(x)?.ToList();
            var lettersY = GetLetters(y)?.ToList();

            if (lettersX is null && lettersY is null)
                return 0;
            else if (lettersX is null)
                return -1;
            else if (lettersY is null)
                return 1;

            int maxLength = Min(lettersX.Count, lettersY.Count);
            if (maxLength == 0)
                return lettersX.Count - lettersY.Count;

            int result = 0;
            var functions = new Func<LetterInfo, int>[]
            {
               new (x => x.Primary),
               new (x => x.Secondary),
               new (x => x.Tertiary),
               new (x => x.Quaternary),
            };
            for (int fn = 0; result == 0 && fn < functions.Length; fn++)
            {
                var property = functions[fn];
                for (int i = 0; result == 0 && i < maxLength; i++)
                {
                    result = property(lettersX[i]) - property(lettersY[i]);
                }
            }

            if (result == 0)
                result = lettersX.Count.CompareTo(lettersY.Count);

            return result;
        }
    }
}
