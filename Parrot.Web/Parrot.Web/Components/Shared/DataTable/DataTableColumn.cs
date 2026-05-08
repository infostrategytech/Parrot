using Microsoft.AspNetCore.Components;

namespace Parrot.Web.Components.Shared;

public class DataTableColumn<TItem>
{
    public string Header { get; set; } = string.Empty;
    public string HeaderStyle { get; set; } = string.Empty;
    public string CellStyle { get; set; } = string.Empty;
    public required RenderFragment<TItem> Template { get; set; }
}
