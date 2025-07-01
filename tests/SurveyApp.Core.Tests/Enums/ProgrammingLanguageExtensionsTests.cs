using FluentAssertions;
using SurveyApp.Core.Enums;

namespace SurveyApp.Core.Tests.Enums;

public class ProgrammingLanguageExtensionsTests
{
    [Theory]
    [InlineData("PHP", ProgrammingLanguage.PHP)]
    [InlineData("JavaScript", ProgrammingLanguage.JavaScript)]
    [InlineData("C", ProgrammingLanguage.C)]
    [InlineData("C++", ProgrammingLanguage.CPlusPlus)]
    [InlineData("Java", ProgrammingLanguage.Java)]
    [InlineData("C#", ProgrammingLanguage.CSharp)]
    [InlineData("Python", ProgrammingLanguage.Python)]
    [InlineData("Ruby", ProgrammingLanguage.Ruby)]
    public void TryParse_ValidLanguage_ShouldReturnTrueAndCorrectLanguage(string input, ProgrammingLanguage expected)
    {
        // Act
        var result = ProgrammingLanguageExtensions.TryParse(input, out var language);

        // Assert
        result.Should().BeTrue();
        language.Should().Be(expected);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("")]
    [InlineData("c#")] // lowercase
    [InlineData("javascript")] // lowercase
    [InlineData("C++")] // different format
    [InlineData("Go")]
    [InlineData("Rust")]
    public void TryParse_InvalidLanguage_ShouldReturnFalse(string input)
    {
        // Act
        var result = ProgrammingLanguageExtensions.TryParse(input, out var language);

        // Assert
        result.Should().BeFalse();
        language.Should().Be(default(ProgrammingLanguage));
    }

    [Theory]
    [InlineData(ProgrammingLanguage.PHP, "PHP")]
    [InlineData(ProgrammingLanguage.JavaScript, "JavaScript")]
    [InlineData(ProgrammingLanguage.C, "C")]
    [InlineData(ProgrammingLanguage.CPlusPlus, "C++")]
    [InlineData(ProgrammingLanguage.Java, "Java")]
    [InlineData(ProgrammingLanguage.CSharp, "C#")]
    [InlineData(ProgrammingLanguage.Python, "Python")]
    [InlineData(ProgrammingLanguage.Ruby, "Ruby")]
    public void ToDisplayString_ShouldReturnCorrectDisplayString(ProgrammingLanguage language, string expected)
    {
        // Act
        var result = language.ToDisplayString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetAllDisplayNames_ShouldReturnAllLanguageNames()
    {
        // Act
        var displayNames = ProgrammingLanguageExtensions.GetAllDisplayNames().ToList();

        // Assert
        displayNames.Should().HaveCount(8);
        displayNames.Should().Contain(new[]
        {
            "PHP", "JavaScript", "C", "C++", "Java", "C#", "Python", "Ruby"
        });
    }

    [Fact]
    public void GetAllDisplayNames_ShouldReturnUniqueNames()
    {
        // Act
        var displayNames = ProgrammingLanguageExtensions.GetAllDisplayNames();

        // Assert
        displayNames.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void TryParse_NullInput_ShouldReturnFalse()
    {
        // Act
        var result = ProgrammingLanguageExtensions.TryParse(null!, out var language);

        // Assert
        result.Should().BeFalse();
        language.Should().Be(default(ProgrammingLanguage));
    }

    [Fact]
    public void TryParse_WhitespaceInput_ShouldReturnFalse()
    {
        // Act
        var result = ProgrammingLanguageExtensions.TryParse("   ", out var language);

        // Assert
        result.Should().BeFalse();
        language.Should().Be(default(ProgrammingLanguage));
    }
}