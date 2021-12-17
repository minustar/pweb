global using System.Collections;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.Text.Json;
global using Microsoft.AspNetCore.Mvc.Rendering;

namespace Minustar.Website.QSyntax;

public partial class QSyntaxParser
{
    public static QSyntaxParser Parse(
        string source,
        TagBuilder? wrap = null
    ) { 
        throw new NotImplementedException();
    }

    private readonly QSyntaxParser instance = new QSyntaxParser();

    private QSyntaxParser()
    {
    }
}
