using System.Net;

namespace Parrot.Domain.Exceptions;

public class ConflictException : DomainException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    public override string ErrorCode => "CONFLICT";

    public ConflictException(string message) : base(message)
    {
    }
}
