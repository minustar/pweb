namespace Minustar.Website.TagHelpers;

[HtmlTargetElement("bs-toolbar")]
public class ToolbarTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        output.TagName = "div";
        output.AddClass("container", NullHtmlEncoder.Default);
        output.AddClass("mb-3", NullHtmlEncoder.Default);
    }
}
