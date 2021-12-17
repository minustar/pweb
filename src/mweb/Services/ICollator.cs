namespace Minustar.Website.Services;

public interface ICollator : IComparer<ISortable>,
                             IComparer<string>
{
    LetterInfo? GetFirstLetter(string? str);
    IEnumerable<LetterInfo>? GetLetters(string? str);
}
