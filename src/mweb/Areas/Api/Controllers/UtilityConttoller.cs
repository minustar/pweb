namespace Minustar.Website.Areas.Api.Controllers;

public abstract class UtilityConttoller : ControllerBase
{
    protected async Task<string?> ReadBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);
        return await reader.ReadToEndAsync();
    }
}
