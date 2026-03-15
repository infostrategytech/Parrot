using System.Net;

namespace Parrot.Domain.Exceptions;

public class ForbiddenException : DomainException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
    public override string ErrorCode => "FORBIDDEN";

    public ForbiddenException(string message = "You do not have permission to access this resource.") 
        : base(message)
    {
    }
}
