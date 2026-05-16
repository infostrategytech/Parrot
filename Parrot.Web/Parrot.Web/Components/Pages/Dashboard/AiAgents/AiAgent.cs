namespace Parrot.Web.Components.Pages.Dashboard.AiAgents
{
    public class AiAgent
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Mascot { get; set; } = "Nova";
        public bool IsActive { get; set; } = true;
        public bool HumanTakeover { get; set; }
        public List<Playbook> Playbooks { get; set; } = new();
        public List<AgentChannel> Channels { get; set; } = new();
        public int MessagesToday { get; set; }
        public int PlaybookCount => Playbooks.Count;
        public int ChannelCount => Channels.Count(c => c.IsEnabled);
    }

    public class Playbook
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StepCount { get; set; }
        public int Execution { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public List<PlaybookStep> Steps { get; set; } = new();
        public List<string> TriggerPhrases { get; set; } = new();
    }

    public class PlaybookStep
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = "AskQuestion";
        public string QuestionText { get; set; } = string.Empty;
        public string ResponseType { get; set; } = string.Empty;
        public string SaveAs { get; set; } = string.Empty;
        public bool Required { get; set; } = true;
        public string Placeholder { get; set; } = string.Empty;
        public int CharacterLimit { get; set; } = 1;
        public int MinValue { get; set; } = 1;
        public int MaxValue { get; set; } = 1000;
        public string Currency { get; set; } = string.Empty;
        public decimal MinAmount { get; set; }
        public List<StepOption> Options { get; set; } = new();
        public List<string> SelectedCatalogItems { get; set; } = new();
        public bool AllowMultiple { get; set; }
        public bool QuantitySelector { get; set; }
        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 10;
        public string NotificationName { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
        public string AmountSource { get; set; } = "Fixed";
        public decimal PaymentAmount { get; set; }
        public string PaymentProvider { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
        public string FailureMessage { get; set; } = string.Empty;
        public string ContactStage { get; set; } = string.Empty;
        public string ContactTags { get; set; } = string.Empty;
        public string WaitDuration { get; set; } = string.Empty;
        public string WaitUnit { get; set; } = "Minutes";
        public string ConditionField { get; set; } = string.Empty;
        public string ConditionOperator { get; set; } = string.Empty;
        public string ConditionValue { get; set; } = string.Empty;
        public string YesAction { get; set; } = string.Empty;
        public string NoAction { get; set; } = string.Empty;
    }

    public class StepOption
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string NextAction { get; set; } = string.Empty;
        public string TargetStep { get; set; } = string.Empty;
    }

    public class AgentChannel
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

    public record PlaybookSaveArgs(string Name, string Description);
}
