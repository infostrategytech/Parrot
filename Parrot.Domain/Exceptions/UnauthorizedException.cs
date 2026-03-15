using System.Net;

namespace Parrot.Domain.Exceptions;

public class UnauthorizedException : DomainException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    public override string ErrorCode => "UNAUTHORIZED";

    public UnauthorizedException(string message = "You are not authorized to perform this action.") 
        : base(message)
    {
    }
}
