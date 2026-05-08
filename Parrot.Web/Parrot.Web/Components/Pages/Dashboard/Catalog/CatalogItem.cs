namespace Parrot.Web.Components.Pages.Dashboard.Catalog;

public class CatalogItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Currency { get; set; } = "NGN";
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
}
