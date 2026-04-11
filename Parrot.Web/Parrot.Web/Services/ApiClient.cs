namespace Parrot.Web.Services;

public class ApiClient
{
    public HttpClient Http { get; }

    public ApiClient(HttpClient httpClient)
    {
        Http = httpClient;
    }
}
