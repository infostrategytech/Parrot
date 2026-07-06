namespace Parrot.Web.Components.Pages.Dashboard.BusinessSetup
{
    public class BusinessOrganization
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string BusinessEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public BusinessSocialMedia SocialMedia { get; set; } = new();
        public List<EmployedAgent> EmployedAgents { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string BrandVoice { get; set; } = string.Empty;
        public List<string> KeyFacts { get; set; } = new();
        public string ExtraContext { get; set; } = string.Empty;
    }

    public class BusinessSocialMedia
    {
        public string Facebook { get; set; } = string.Empty;
        public string Instagram { get; set; } = string.Empty;
        public string TwitterX { get; set; } = string.Empty;
        public string LinkedIn { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public string YouTube { get; set; } = string.Empty;
        public string TikTok { get; set; } = string.Empty;
    }

    public class EmployedAgent
    {
        public string AgentId { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string Mascot { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public static class BusinessIndustries
    {
        public static readonly string[] All =
        [
            "Technology & Software",
            "Retail & E-commerce",
            "Healthcare & Wellness",
            "Finance & Banking",
            "Education & Training",
            "Food & Beverage",
            "Real Estate",
            "Travel & Hospitality",
            "Logistics & Delivery",
            "Media & Entertainment",
            "Professional Services",
            "Manufacturing",
            "Non-profit",
            "Other"
        ];
    }

    public static class BusinessSizes
    {
        public static readonly string[] All =
        [
            "Solo (just me)",
            "2 – 10 employees",
            "11 – 50 employees",
            "51 – 200 employees",
            "201 – 1,000 employees",
            "1,000+ employees"
        ];
    }

    public static class AgentRoles
    {
        public static readonly string[] All =
        [
            "Customer Support",
            "Sales",
            "Billing & Payments",
            "Lead Qualification",
            "Booking & Scheduling",
            "Product Information",
            "General Assistant"
        ];
    }
}
