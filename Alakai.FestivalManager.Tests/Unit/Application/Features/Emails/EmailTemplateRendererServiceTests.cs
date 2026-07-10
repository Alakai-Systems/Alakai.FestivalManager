using Alakai.FestivalManager.Application.Features.Emails.Services;

namespace Alakai.FestivalManager.Tests.Unit.Application.Features.Emails;

public class EmailTemplateRendererServiceTests
{
    private readonly EmailTemplateRendererService _sut = new();

    // ── Basic substitution ────────────────────────────────────────────────────

    [Fact]
    public void Render_WhenTemplateIsEmpty_ReturnsEmptyString()
    {
        string result = _sut.Render(string.Empty, new Dictionary<string, string>());

        result.Should().BeEmpty();
    }

    [Fact]
    public void Render_WhenTemplateIsWhitespace_ReturnsEmptyString()
    {
        string result = _sut.Render("   ", new Dictionary<string, string>());

        result.Should().BeEmpty();
    }

    [Fact]
    public void Render_WhenVariableExists_SubstitutesPlaceholder()
    {
        string template = "Dear {{FirstName}},";
        Dictionary<string, string> variables = new() { ["FirstName"] = "Mago" };

        string result = _sut.Render(template, variables);

        result.Should().Be("Dear Mago,");
    }

    [Fact]
    public void Render_WhenMultipleVariables_SubstitutesAll()
    {
        string template = "Dear {{FirstName}} {{LastName}}, your edition is {{EditionName}}.";
        Dictionary<string, string> variables = new()
        {
            ["FirstName"] = "Jose",
            ["LastName"] = "Farfan",
            ["EditionName"] = "Swim Out Costa Brava 2026"
        };

        string result = _sut.Render(template, variables);

        result.Should().Be("Dear Jose Farfan, your edition is Swim Out Costa Brava 2026.");
    }

    [Fact]
    public void Render_WhenVariableValueIsNull_ReplacesWithEmptyString()
    {
        string template = "Hello {{FirstName}}!";
        Dictionary<string, string> variables = new() { ["FirstName"] = null! };

        string result = _sut.Render(template, variables);

        result.Should().Be("Hello !");
    }

    [Fact]
    public void Render_WhenPlaceholderHasNoMatchingVariable_LeavesPlaceholderEmpty()
    {
        string template = "Hello {{FirstName}}!";
        Dictionary<string, string> variables = new();

        string result = _sut.Render(template, variables);

        // Placeholder with no variable → stays as-is (not substituted)
        result.Should().Be("Hello {{FirstName}}!");
    }

    // ── HTML tag stripping inside placeholders (WYSIWYG editor fix) ───────────

    [Fact]
    public void Render_WhenPlaceholderContainsBoldTag_StripsTagAndSubstitutes()
    {
        string template = "Stay at {{Accommodation<b>BuildingName</b>}}.";
        Dictionary<string, string> variables = new() { ["AccommodationBuildingName"] = "Kim's Bungalows" };

        string result = _sut.Render(template, variables);

        result.Should().Be("Stay at Kim's Bungalows.");
    }

    [Fact]
    public void Render_WhenPlaceholderContainsSpanTag_StripsTagAndSubstitutes()
    {
        string template = "Hello {{<span>FirstName</span>}},";
        Dictionary<string, string> variables = new() { ["FirstName"] = "Mago" };

        string result = _sut.Render(template, variables);

        result.Should().Be("Hello Mago,");
    }

    [Fact]
    public void Render_WhenPlaceholderContainsNestedTags_StripsAllTagsAndSubstitutes()
    {
        string template = "Edition: {{<strong><em>EditionName</em></strong>}}";
        Dictionary<string, string> variables = new() { ["EditionName"] = "Swim Out 2026" };

        string result = _sut.Render(template, variables);

        result.Should().Be("Edition: Swim Out 2026");
    }

    [Fact]
    public void Render_WhenPlaceholderContainsHtmlEntity_DecodesAndSubstitutes()
    {
        string template = "Hello {{First&amp;Name}},";
        Dictionary<string, string> variables = new() { ["First&Name"] = "Mago" };

        string result = _sut.Render(template, variables);

        result.Should().Be("Hello Mago,");
    }

    [Fact]
    public void Render_WhenPlaceholderHasExtraWhitespace_TrimsAndSubstitutes()
    {
        string template = "Hello {{ FirstName }},";
        Dictionary<string, string> variables = new() { ["FirstName"] = "Mago" };

        string result = _sut.Render(template, variables);

        result.Should().Be("Hello Mago,");
    }

    // ── Template with no placeholders ─────────────────────────────────────────

    [Fact]
    public void Render_WhenTemplateHasNoPlaceholders_ReturnsTemplateUnchanged()
    {
        string template = "Best wishes, the team.";
        Dictionary<string, string> variables = new() { ["FirstName"] = "Mago" };

        string result = _sut.Render(template, variables);

        result.Should().Be("Best wishes, the team.");
    }
}
