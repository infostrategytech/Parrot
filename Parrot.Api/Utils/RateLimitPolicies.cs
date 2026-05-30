namespace Parrot.Api.Utils;

public static class RateLimitPolicies
{
    public const string Register = "auth:register";
    public const string Login = "auth:login";
    public const string VerifyEmail = "auth:verify-email";
    public const string ResendVerification = "auth:resend-verification";
    public const string ForgotPassword = "auth:forgot-password";
    public const string ResetPassword = "auth:reset-password";
    public const string ChangePassword = "auth:change-password";
    public const string WhatsAppWebhook = "webhooks:whatsapp";
    public const string FacebookWebhook = "webhooks:facebook";
}
