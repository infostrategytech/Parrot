namespace Parrot.Web.Components.Pages.Dashboard.Catalog;

public class CatalogCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}
