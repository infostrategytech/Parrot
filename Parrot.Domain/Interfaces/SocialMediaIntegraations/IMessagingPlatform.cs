using System;

namespace Parrot.Domain.Interfaces.SocialMediaIntegraations;

public interface IMessagingPlatform
{
    Task SendMessageAsync(string recipient, string message);
    Task ProcessIncomingMessageAsync(string sender, string message);    


}
