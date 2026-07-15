using ShipFood.Controllers;

namespace ShipFoodCore.Tests.Controllers;

/// <summary>
/// Tests for HomeController static utility methods.
/// These are pure functions that can be tested without DB or HTTP context.
/// </summary>
public class HomeControllerStaticTests
{
    // ═══════════════════════════════════════════════════════
    // RemoveDiacritics tests
    // ═══════════════════════════════════════════════════════

    [Theory]
    [InlineData("Hà Nội", "Ha Noi")]
    [InlineData("Đà Nẵng", "Da Nang")]
    [InlineData("TP. Hồ Chí Minh", "TP. Ho Chi Minh")]
    [InlineData("Phở Bò", "Pho Bo")]
    [InlineData("Bún Chả Hà Nội", "Bun Cha Ha Noi")]
    public void RemoveDiacritics_VietnameseText_RemovesAccents(string input, string expected)
    {
        var result = HomeController.RemoveDiacritics(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RemoveDiacritics_EmptyString_ReturnsEmpty()
    {
        Assert.Equal("", HomeController.RemoveDiacritics(""));
    }

    [Fact]
    public void RemoveDiacritics_NullString_ReturnsEmpty()
    {
        Assert.Equal("", HomeController.RemoveDiacritics(null!));
    }

    [Fact]
    public void RemoveDiacritics_AspNet_ReturnsAspNet()
    {
        Assert.Equal("ASP.NET", HomeController.RemoveDiacritics("ASP.NET"));
    }

    [Fact]
    public void RemoveDiacritics_SpecialChars_Preserved()
    {
        Assert.Equal("abc!@#$%", HomeController.RemoveDiacritics("abc!@#$%"));
    }

    // ═══════════════════════════════════════════════════════
    // ValidatePasswordStrength tests (private static method)
    // Test via reflection
    // ═══════════════════════════════════════════════════════

    private static (bool Valid, string Message) ValidatePasswordStrength(string password)
    {
        var method = typeof(HomeController).GetMethod("ValidatePasswordStrength",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return ((bool, string))method!.Invoke(null, new object[] { password })!;
    }

    [Theory]
    [InlineData("Abcd1234!", true)]
    [InlineData("Str0ng@Pass", true)]
    [InlineData("MyP@ssw0rd1", true)]
    public void ValidatePasswordStrength_StrongPassword_ReturnsValid(string password, bool expected)
    {
        var (valid, _) = ValidatePasswordStrength(password);
        Assert.Equal(expected, valid);
    }

    [Theory]
    [InlineData("short", false)]           // Too short
    [InlineData("alllowercase1!", false)]  // No uppercase
    [InlineData("ALLUPPERCASE1!", false)]  // No lowercase
    [InlineData("NoDigitsHere!", false)]   // No digit
    [InlineData("NoSpecialChar1", false)]  // No special char
    [InlineData("12345678", false)]        // Only digits, too short
    public void ValidatePasswordStrength_WeakPassword_ReturnsInvalid(string password, bool expected)
    {
        var (valid, _) = ValidatePasswordStrength(password);
        Assert.Equal(expected, valid);
    }

    [Fact]
    public void ValidatePasswordStrength_EmptyPassword_ReturnsInvalid()
    {
        var (valid, message) = ValidatePasswordStrength("");
        Assert.False(valid);
        Assert.Contains("8 ký tự", message);
    }

    [Fact]
    public void ValidatePasswordStrength_ExactlyMinLength_ReturnsValid()
    {
        var (valid, _) = ValidatePasswordStrength("Abc123!@");
        Assert.True(valid);
    }

    [Fact]
    public void ValidatePasswordStrength_SevenChars_ReturnsInvalid()
    {
        var (valid, _) = ValidatePasswordStrength("Abc12!@");
        Assert.False(valid);
    }

    [Fact]
    public void ValidatePasswordStrength_OverMax_ReturnsInvalid()
    {
        var longPwd = new string('A', 129) + "bc1234!"; // 136 chars
        var (valid, message) = ValidatePasswordStrength(longPwd);
        Assert.False(valid);
        Assert.Contains("128 ký tự", message);
    }

    [Fact]
    public void ValidatePasswordStrength_MaxLengthExactly_ReturnsValid()
    {
        var pwd = new string('A', 124) + "abc123!"; // 131 — too long, should be invalid  
        // Wait: 124+7=131 > 128. Let me recalculate:
        // 125 uppercase + "b1234!" = 125+6 = 131 > 128. 
        // Let's test at exactly 128 chars:
        var exactlyMax = new string('A', 120) + "bc1234!"; // 120+7=127 < 128
        var (valid, _) = ValidatePasswordStrength(exactlyMax);
        Assert.True(valid, "127-char password with all requirements should be valid");

        var atMax = new string('A', 121) + "bc1234!"; // 121+7=128 = exactly 128
        var (valid2, _) = ValidatePasswordStrength(atMax);
        Assert.True(valid2, "128-char password should be valid (not exceeding)");
    }
}