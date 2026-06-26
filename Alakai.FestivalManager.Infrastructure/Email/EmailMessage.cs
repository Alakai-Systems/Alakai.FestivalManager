namespace Alakai.FestivalManager.Infrastructure.Email;

public class EmailMessage
{
    public EmailAddress To { get; set; } = new();
    public List<EmailAddress> Cc { get; set; } = [];
    public List<EmailAddress> Bcc { get; set; } = [];
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string TextBody { get; set; } = string.Empty;
    public List<EmailAttachment> Attachments { get; set; } = [];
}
