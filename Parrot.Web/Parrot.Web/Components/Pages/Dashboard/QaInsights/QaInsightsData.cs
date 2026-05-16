namespace Parrot.Web.Components.Pages.Dashboard.QaInsights;

public record StatCard(string Title, string Value, string Sub, string Change, bool ChangePositive);

public record PerformanceRow(string Label, string Value);

public record SentimentSlice(string Label, string Color, double Percent);
