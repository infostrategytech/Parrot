namespace Parrot.Web.Features.Dashboard.QaInsights.State
{
    public class QaInsightsState
    {
        public bool IsLoading { get; private set; }

        public void SetLoading(bool value) => IsLoading = value;
    }
}
 