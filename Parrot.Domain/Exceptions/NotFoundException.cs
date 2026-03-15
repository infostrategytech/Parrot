using System.Net;

namespace Parrot.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public override string ErrorCode => "NOT_FOUND";

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}
