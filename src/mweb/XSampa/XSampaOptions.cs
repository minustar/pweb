namespace Minustar.Website.XSampa;

public class XSampaOptions
{
    public XSampaDialect Dialect { get; set; }
    public bool UseAffricateLigatures { get; set; }

    public XSampaOptions()
    {
        Dialect = XSampaDialect.XSampa;
        UseAffricateLigatures = false;
    }
}
