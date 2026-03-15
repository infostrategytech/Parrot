namespace Parrot.SharedComponents.Components.InputFields
{
    public class FormInputModel
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = "text";
        public string Placeholder { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Disabled { get; set; }
        public bool ShowIcon { get; set; } = true;
    }
}
