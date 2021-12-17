namespace Minustar.Website.QSyntax;

public class QSyntaxParserOptions
{
    public string? OuterTag = null;
    public ArticleUriGenerator ArticleLinkGenerator { get; set; }
    public EntryCrossReferenceUriGenerator EntryCrossReferenceLinkGenerator { get; set; }

    public QSyntaxParserOptions()
    {
        this.ArticleLinkGenerator = (article_id, kind) => new Uri("/");
        this.EntryCrossReferenceLinkGenerator = (target, lang_id) => new Uri("/");
    }

    /// <summary>
    /// Represents a method delegate that produces an <see cref="Uri"/>
    /// from the ID of an aricle (e.g. its title or an alias).
    /// </summary>
    /// <param name="article_id">
    /// The ID, or alias, of the article.
    /// </param>
    /// <param name="kind">
    /// The kind of link, if this is a reference to a
    /// specific keyword, or a search item.
    /// </param>
    /// <returns>
    /// An <see cref="Uri"/>.
    /// </returns>
    public delegate Uri ArticleUriGenerator(string article_id, ArticleUriKind kind = ArticleUriKind.Article);

    /// <summary>
    /// Represents a method delegates that produces an <see cref="Uri"/>
    /// from an <see cref="DictionaryEntry"/>'s target hash-
    /// </summary>
    /// <param name="target">
    /// The target hash.
    /// </param>
    /// <param name="lang_id">
    /// The ID of the language.
    /// </param>
    /// <returns>
    /// An <see cref="Uri"/>.
    /// </returns>
    public delegate Uri EntryCrossReferenceUriGenerator(string target, string lang_id);
}
