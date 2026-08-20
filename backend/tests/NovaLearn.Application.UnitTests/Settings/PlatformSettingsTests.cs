using FluentAssertions;
using NovaLearn.Domain.Settings;
using Xunit;

namespace NovaLearn.Application.UnitTests.Settings;

public sealed class PlatformSettingsTests
{
    [Fact]
    public void The_default_row_opens_registration_and_leaves_maintenance_off()
    {
        PlatformSettings settings = PlatformSettings.CreateDefault();

        settings.Id.Should().Be(PlatformSettings.SingletonId);
        settings.AllowNewRegistrations.Should().BeTrue();
        settings.MaintenanceModeEnabled.Should().BeFalse();
        settings.DefaultCurrency.Should().Be("usd");
    }

    private static PlatformSettings Defaults() => PlatformSettings.CreateDefault();

    [Fact]
    public void Updating_applies_every_field_and_trims_text()
    {
        PlatformSettings settings = Defaults();

        settings.Update(
            "  Springfield University  ",
            "  help@springfield.edu  ",
            allowNewRegistrations: false,
            maintenanceModeEnabled: true,
            "  Back shortly.  ",
            "EUR",
            50);

        settings.SiteName.Should().Be("Springfield University");
        settings.SupportEmail.Should().Be("help@springfield.edu");
        settings.AllowNewRegistrations.Should().BeFalse();
        settings.MaintenanceModeEnabled.Should().BeTrue();
        settings.MaintenanceMessage.Should().Be("Back shortly.");

        // Stripe's own convention is lower case, whatever case the admin typed.
        settings.DefaultCurrency.Should().Be("eur");
        settings.MaxUploadSizeMb.Should().Be(50);
    }

    [Fact]
    public void A_blank_maintenance_message_is_stored_as_no_message()
    {
        PlatformSettings settings = Defaults();

        settings.Update("Site", "a@b.com", true, true, "   ", "usd", 200);

        settings.MaintenanceMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_empty_site_name_is_refused(string siteName)
    {
        PlatformSettings settings = Defaults();

        Action act = () => settings.Update(siteName, "a@b.com", true, false, null, "usd", 200);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("us")]
    [InlineData("usdd")]
    [InlineData("")]
    public void A_currency_code_that_is_not_exactly_three_characters_is_refused(string currency)
    {
        PlatformSettings settings = Defaults();

        Action act = () => settings.Update("Site", "a@b.com", true, false, null, currency, 200);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    [InlineData(-5)]
    public void An_upload_size_outside_one_to_five_hundred_is_refused(int megabytes)
    {
        PlatformSettings settings = Defaults();

        Action act = () => settings.Update("Site", "a@b.com", true, false, null, "usd", megabytes);

        act.Should().Throw<ArgumentException>();
    }
}
