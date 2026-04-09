namespace Parrot.Web.Features.Dashboard.Catalog.State
{
    public class CatalogState
    {
        public bool IsLoading { get; private set; }

        public void SetLoading(bool value) => IsLoading = value;
    }
}
