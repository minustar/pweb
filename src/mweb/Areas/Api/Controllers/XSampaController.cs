namespace Minustar.Website.Areas.Api.Controllers;

[Area("API")]
[ApiController]
[Route("api/[controller]")]
public class XSampaController : UtilityConttoller
{
    [Route("ipa")]
    [HttpGet]
    [HttpPost]
    public async Task<string?> COnvertToIpa(string? text = null)
    {
        if (text is null && HttpContext.Request.Method == "POST")
            text = await ReadBodyAsync();

        try
        {
            var result = text?.ParseXSampa();
            return result;
        }
        catch
        {
            return text;
        }
    }
}
