namespace Parrot.Web.Components.Shared;

public class DropdownOption
{
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";

    public DropdownOption() { }

    public DropdownOption(string value, string label)
    {
        Value = value;
        Label = label;
    }
}
