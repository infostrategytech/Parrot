using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Parrot.Api.DTOs.Auth;
using Parrot.Api.Logging;
using Parrot.Api.Models;
using Parrot.Api.Services.Auth;
using Parrot.Api.Services.Email;
using Parrot.Api.Utils;
using Parrot.Domain.Exceptions;

namespace Test.Parrot.Api.Tests.Services.Auth;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtHelper> _jwtHelperMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IAppLogger<AuthService>> _loggerMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        Mock<IUserStore<ApplicationUser>> store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _jwtHelperMock = new Mock<IJwtHelper>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<IAppLogger<AuthService>>();

        _loggerMock
            .Setup(l => l.BeginEntityScope(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Mock.Of<IDisposable>());

        _jwtHelperMock
            .Setup(j => j.GetDefaultExpiration())
            .Returns(DateTime.UtcNow.AddHours(1));

        _jwtHelperMock
            .Setup(j => j.GenerateToken(It.IsAny<IEnumerable<Claim>>(), It.IsAny<DateTime?>()))
            .Returns("test-jwt-token");

        IOptions<EmailSettings> emailSettings = Options.Create(new EmailSettings
        {
            SmtpHost = "localhost",
            SmtpPort = 587,
            SmtpUser = "user",
            SmtpPassword = "pass",
            FromAddress = "from@test.com",
            FromName = "Test",
            BaseUrl = "http://localhost:3000",
        });

        _sut = new AuthService(
            _userManagerMock.Object,
            _jwtHelperMock.Object,
            _emailServiceMock.Object,
            emailSettings,
            _loggerMock.Object);
    }

    // ---------------------------------------------------------------------------
    // LoginAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task LoginAsync_WhenAccountIsLockedOut_ThrowsUnauthorizedException()
    {
        ApplicationUser user = CreateUser(emailConfirmed: true);
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true);

        UnauthorizedException ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = user.Email!, Password = "any" }));

        Assert.Contains("locked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorizedException()
    {
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "nobody@test.com", Password = "pass" }));
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_DoesNotCallAccessFailed()
    {
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "nobody@test.com", Password = "pass" }));

        _userManagerMock.Verify(m => m.AccessFailedAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordInvalid_ThrowsUnauthorizedException_AndIncrementsFailedCount()
    {
        ApplicationUser user = CreateUser(emailConfirmed: true);
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);
        _userManagerMock.Setup(m => m.AccessFailedAsync(user)).ReturnsAsync(IdentityResult.Success);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = user.Email!, Password = "wrong" }));

        _userManagerMock.Verify(m => m.AccessFailedAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenEmailNotConfirmed_ThrowsUnauthorizedException()
    {
        ApplicationUser user = CreateUser(emailConfirmed: false);
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "correct")).ReturnsAsync(true);

        UnauthorizedException ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = user.Email!, Password = "correct" }));

        Assert.Contains("verify your email", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsValid_ReturnsAuthResponse_AndResetsAccessFailed()
    {
        ApplicationUser user = CreateUser(emailConfirmed: true);
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "correct")).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

        AuthResponse response = await _sut.LoginAsync(
            new LoginRequest { Email = user.Email!, Password = "correct" });

        Assert.Equal("test-jwt-token", response.Token);
        Assert.Equal(user.Id, response.UserId);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal(user.BusinessName, response.BusinessName);
        _userManagerMock.Verify(m => m.ResetAccessFailedCountAsync(user), Times.Once);
    }

    // ---------------------------------------------------------------------------
    // RegisterAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyRegistered_ThrowsConflictException()
    {
        ApplicationUser existing = CreateUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync(existing.Email!)).ReturnsAsync(existing);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.RegisterAsync(new RegisterRequest
            {
                Email = existing.Email!,
                Password = "Pass123!",
                ConfirmPassword = "Pass123!",
                BusinessName = "Biz",
            }));
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyRegistered_DoesNotSendVerificationEmail()
    {
        ApplicationUser existing = CreateUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync(existing.Email!)).ReturnsAsync(existing);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.RegisterAsync(new RegisterRequest
            {
                Email = existing.Email!,
                Password = "Pass123!",
                ConfirmPassword = "Pass123!",
                BusinessName = "Biz",
            }));

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenIdentityCreationFails_ThrowsValidationException()
    {
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.RegisterAsync(new RegisterRequest
            {
                Email = "new@test.com",
                Password = "Pass123!",
                ConfirmPassword = "Pass123!",
                BusinessName = "Biz",
            }));

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenSuccessful_SendsVerificationEmail_AndReturnsResponse()
    {
        _userManagerMock
            .Setup(m => m.FindByEmailAsync("new@test.com"))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "Pass123!"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("confirm-token");
        _emailServiceMock
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        AuthResponse response = await _sut.RegisterAsync(new RegisterRequest
        {
            Email = "new@test.com",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!",
            BusinessName = "My Biz",
        });

        Assert.Equal("test-jwt-token", response.Token);
        Assert.Equal("new@test.com", response.Email);
        Assert.Equal("My Biz", response.BusinessName);

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(
                "new@test.com",
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains("http://localhost:3000"))),
            Times.Once);
    }

    // ---------------------------------------------------------------------------
    // VerifyEmailAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task VerifyEmailAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        _userManagerMock
            .Setup(m => m.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.VerifyEmailAsync(new VerifyEmailRequest { UserId = "missing-id", Token = "tok" }));
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenTokenInvalid_ThrowsValidationException()
    {
        ApplicationUser user = CreateUser();
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.ConfirmEmailAsync(user, "bad-token"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.VerifyEmailAsync(new VerifyEmailRequest { UserId = user.Id, Token = "bad-token" }));
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenTokenValid_ReturnsAuthResponse()
    {
        ApplicationUser user = CreateUser();
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.ConfirmEmailAsync(user, "good-token"))
            .ReturnsAsync(IdentityResult.Success);

        AuthResponse response = await _sut.VerifyEmailAsync(
            new VerifyEmailRequest { UserId = user.Id, Token = "good-token" });

        Assert.Equal("test-jwt-token", response.Token);
        Assert.Equal(user.Id, response.UserId);
        Assert.Equal(user.Email, response.Email);
    }

    // ---------------------------------------------------------------------------
    // ResendVerificationEmailAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenUserNotFound_DoesNotSendEmail()
    {
        _userManagerMock
            .Setup(m => m.FindByEmailAsync("nobody@test.com"))
            .ReturnsAsync((ApplicationUser?)null);

        await _sut.ResendVerificationEmailAsync(
            new ResendVerificationEmailRequest { Email = "nobody@test.com" });

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenEmailAlreadyConfirmed_DoesNotSendEmail()
    {
        ApplicationUser user = CreateUser(emailConfirmed: true);
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        await _sut.ResendVerificationEmailAsync(
            new ResendVerificationEmailRequest { Email = user.Email! });

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_WhenUnconfirmed_SendsVerificationEmail()
    {
        ApplicationUser user = CreateUser(emailConfirmed: false);
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("new-token");
        _emailServiceMock
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.ResendVerificationEmailAsync(
            new ResendVerificationEmailRequest { Email = user.Email! });

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(user.Email!, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------------
    // ForgotPasswordAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ForgotPasswordAsync_WhenUserNotFound_DoesNotSendEmail()
    {
        _userManagerMock
            .Setup(m => m.FindByEmailAsync("nobody@test.com"))
            .ReturnsAsync((ApplicationUser?)null);

        await _sut.ForgotPasswordAsync(new ForgotPasswordRequest { Email = "nobody@test.com" });

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WhenUserExists_SendsResetEmail()
    {
        ApplicationUser user = CreateUser(emailConfirmed: true);
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");
        _emailServiceMock
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.ForgotPasswordAsync(new ForgotPasswordRequest { Email = user.Email! });

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(
                user.Email!,
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains("reset-password"))),
            Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ResetUrlContainsUserIdAndEncodedToken()
    {
        ApplicationUser user = CreateUser(emailConfirmed: true);
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("raw+token=");
        _emailServiceMock
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.ForgotPasswordAsync(new ForgotPasswordRequest { Email = user.Email! });

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(
                user.Email!,
                It.IsAny<string>(),
                It.Is<string>(body =>
                    body.Contains(user.Id) &&
                    body.Contains(Uri.EscapeDataString("raw+token=")))),
            Times.Once);
    }

    // ---------------------------------------------------------------------------
    // ResetPasswordAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ResetPasswordAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        _userManagerMock
            .Setup(m => m.FindByIdAsync("missing"))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ResetPasswordAsync(new ResetPasswordRequest
            {
                UserId = "missing",
                Token = "tok",
                NewPassword = "NewPass1!",
                ConfirmNewPassword = "NewPass1!",
            }));
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenTokenInvalid_ThrowsValidationException()
    {
        ApplicationUser user = CreateUser();
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.ResetPasswordAsync(user, "bad", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.ResetPasswordAsync(new ResetPasswordRequest
            {
                UserId = user.Id,
                Token = "bad",
                NewPassword = "NewPass1!",
                ConfirmNewPassword = "NewPass1!",
            }));
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenTokenValid_CompletesSuccessfully()
    {
        ApplicationUser user = CreateUser();
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.ResetPasswordAsync(user, "good-token", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserId = user.Id,
            Token = "good-token",
            NewPassword = "NewPass1!",
            ConfirmNewPassword = "NewPass1!",
        });

        _userManagerMock.Verify(m => m.ResetPasswordAsync(user, "good-token", "NewPass1!"), Times.Once);
    }

    // ---------------------------------------------------------------------------
    // ChangePasswordAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ChangePasswordAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        _userManagerMock
            .Setup(m => m.FindByIdAsync("unknown"))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ChangePasswordAsync("unknown", new ChangePasswordRequest
            {
                CurrentPassword = "OldPass1!",
                NewPassword = "NewPass1!",
                ConfirmNewPassword = "NewPass1!",
            }));
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordWrong_ThrowsValidationException()
    {
        ApplicationUser user = CreateUser();
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.ChangePasswordAsync(user, "WrongOld1!", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Wrong password" }));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.ChangePasswordAsync(user.Id, new ChangePasswordRequest
            {
                CurrentPassword = "WrongOld1!",
                NewPassword = "NewPass1!",
                ConfirmNewPassword = "NewPass1!",
            }));
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordCorrect_CompletesSuccessfully()
    {
        ApplicationUser user = CreateUser();
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.ChangePasswordAsync(user, "OldPass1!", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.ChangePasswordAsync(user.Id, new ChangePasswordRequest
        {
            CurrentPassword = "OldPass1!",
            NewPassword = "NewPass1!",
            ConfirmNewPassword = "NewPass1!",
        });

        _userManagerMock.Verify(m => m.ChangePasswordAsync(user, "OldPass1!", "NewPass1!"), Times.Once);
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static ApplicationUser CreateUser(bool emailConfirmed = false) =>
        new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user@test.com",
            UserName = "user@test.com",
            BusinessName = "Test Business",
            EmailConfirmed = emailConfirmed,
        };
}
