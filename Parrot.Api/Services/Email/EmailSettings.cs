namespace Parrot.Api.Services.Email;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    required public string SmtpHost { get; init; }

    public int SmtpPort { get; init; } = 587;

    required public string SmtpUser { get; init; }

    required public string SmtpPassword { get; init; }

    required public string FromAddress { get; init; }

    required public string FromName { get; init; }

    required public string BaseUrl { get; init; }
}
