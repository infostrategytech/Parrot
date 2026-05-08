namespace Parrot.Web.Components.Pages.Dashboard.Contacts;

public class Contact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ChannelIdentifier { get; set; } = string.Empty;
    public bool HasWhatsApp { get; set; }
    public bool HasInstagram { get; set; }
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string InstagramHandle { get; set; } = string.Empty;
    public string LastInteraction { get; set; } = "5 minutes ago";
    public string LastInteractionChannel { get; set; } = "whatsapp";
    public List<string> Tags { get; set; } = new();
    public int OrderCount { get; set; }
    public string Stage { get; set; } = string.Empty;
    public List<ContactOrder> Orders { get; set; } = new();
}
