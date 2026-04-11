namespace Parrot.Web.Features.Dashboard.Contacts.State
{
    public class ContactsState
    {
        public bool IsLoading { get; private set; }

        public void SetLoading(bool value) => IsLoading = value;
    }
}
