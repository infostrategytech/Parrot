using Microsoft.Extensions.Options;
using Moq;
using Parrot.Application.DTOs.Integrations;
using Parrot.Application.Integrations;
using Parrot.Application.Logging;
using Parrot.Domain.Enums;

namespace Test.Parrot.Api.Tests.Services.Integrations;

public class WhatsAppWebhookServiceTests
{
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly Mock<IAppLogger<WhatsAppWebhookService>> _loggerMock;
    private readonly WhatsAppWebhookService _sut;

    public WhatsAppWebhookServiceTests()
    {
        _publisherMock = new Mock<IMessagePublisher>();
        _loggerMock = new Mock<IAppLogger<WhatsAppWebhookService>>();

        _loggerMock
            .Setup(l => l.BeginEntityScope(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Mock.Of<IDisposable>());

        IOptions<WhatsAppSettings> settings = Options.Create(new WhatsAppSettings
        {
            WebhookBaseUrl = "https://api.example.com",
            VerifyToken = "test-verify-token",
            AppSecret = "test-secret",
        });

        _sut = new WhatsAppWebhookService(settings, _loggerMock.Object, _publisherMock.Object);
    }

    [Fact]
    public void GetWebhookUrl_ReturnsUrlWithTrailingSlashStripped()
    {
        string url = _sut.GetWebhookUrl();

        Assert.Equal("https://api.example.com/api/webhooks/whatsapp", url);
    }

    [Fact]
    public void TryVerifyChallenge_ValidModeAndToken_ReturnsTrueAndEchoesChallenge()
    {
        bool result = _sut.TryVerifyChallenge("subscribe", "test-verify-token", "challenge-xyz", out string echo);

        Assert.True(result);
        Assert.Equal("challenge-xyz", echo);
    }

    [Fact]
    public void TryVerifyChallenge_WrongMode_ReturnsFalseAndEmptyEcho()
    {
        bool result = _sut.TryVerifyChallenge("unsubscribe", "test-verify-token", "challenge-xyz", out string echo);

        Assert.False(result);
        Assert.Empty(echo);
    }

    [Fact]
    public void TryVerifyChallenge_WrongToken_ReturnsFalseAndEmptyEcho()
    {
        bool result = _sut.TryVerifyChallenge("subscribe", "wrong-token", "challenge-xyz", out string echo);

        Assert.False(result);
        Assert.Empty(echo);
    }

    [Fact]
    public async Task ProcessEventAsync_SingleTextMessage_PublishesOneBatch()
    {
        WhatsAppWebhookPayload payload = BuildPayload(
            entryId: "business-account-1",
            phoneNumberId: "phone-123",
            displayPhone: "+1 555 000 1111",
            messages: [("15550001111", "msg-001", "1700000000", "text", "Hello!")]);

        await _sut.ProcessEventAsync(payload);

        _publisherMock.Verify(
            p => p.PublishBatchAsync(
                It.Is<IReadOnlyList<IncomingMessage>>(list =>
                    list.Count == 1 &&
                    list[0].MessageId == "msg-001" &&
                    list[0].Platform == MessagingPlatform.WhatsApp &&
                    list[0].From == "15550001111" &&
                    list[0].TextBody == "Hello!" &&
                    list[0].ChannelId == "phone-123" &&
                    list[0].ExternalAccountId == "business-account-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessEventAsync_MultipleMessages_PublishesAllInSingleBatch()
    {
        WhatsAppWebhookPayload payload = BuildPayload(
            entryId: "business-account-1",
            phoneNumberId: "phone-123",
            displayPhone: "+1 555 000 1111",
            messages:
            [
                ("15550001111", "msg-001", "1700000001", "text", "Hello!"),
                ("15550002222", "msg-002", "1700000002", "text", "World!"),
            ]);

        await _sut.ProcessEventAsync(payload);

        _publisherMock.Verify(
            p => p.PublishBatchAsync(
                It.Is<IReadOnlyList<IncomingMessage>>(list => list.Count == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessEventAsync_ContactPresent_MapsContactNameToMessage()
    {
        WhatsAppWebhookPayload payload = BuildPayloadWithContact(
            from: "15550001111",
            messageId: "msg-001",
            contactName: "John Doe");

        await _sut.ProcessEventAsync(payload);

        _publisherMock.Verify(
            p => p.PublishBatchAsync(
                It.Is<IReadOnlyList<IncomingMessage>>(list =>
                    list.Count == 1 &&
                    list[0].ContactName == "John Doe"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessEventAsync_NoContactForSender_ContactNameIsNull()
    {
        WhatsAppWebhookPayload payload = BuildPayload(
            entryId: "acct-1",
            phoneNumberId: "phone-1",
            displayPhone: "+1",
            messages: [("15550001111", "msg-001", "1700000000", "text", "Hi")]);

        await _sut.ProcessEventAsync(payload);

        _publisherMock.Verify(
            p => p.PublishBatchAsync(
                It.Is<IReadOnlyList<IncomingMessage>>(list =>
                    list.Count == 1 &&
                    list[0].ContactName == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessEventAsync_NonMessageField_DoesNotPublish()
    {
        WhatsAppWebhookPayload payload = new WhatsAppWebhookPayload
        {
            Object = "whatsapp_business_account",
            Entry =
            [
                new WhatsAppEntry
                {
                    Id = "acct-1",
                    Changes =
                    [
                        new WhatsAppChange
                        {
                            Field = "account_review_update",
                            Value = new WhatsAppChangeValue
                            {
                                MessagingProduct = "whatsapp",
                                Metadata = new WhatsAppMetadata
                                {
                                    PhoneNumberId = "phone-1",
                                    DisplayPhoneNumber = "+1",
                                },
                            },
                        },
                    ],
                },
            ],
        };

        await _sut.ProcessEventAsync(payload);

        _publisherMock.Verify(
            p => p.PublishBatchAsync(It.IsAny<IReadOnlyList<IncomingMessage>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessEventAsync_StatusesOnly_DoesNotPublish()
    {
        WhatsAppWebhookPayload payload = new WhatsAppWebhookPayload
        {
            Object = "whatsapp_business_account",
            Entry =
            [
                new WhatsAppEntry
                {
                    Id = "acct-1",
                    Changes =
                    [
                        new WhatsAppChange
                        {
                            Field = "messages",
                            Value = new WhatsAppChangeValue
                            {
                                MessagingProduct = "whatsapp",
                                Metadata = new WhatsAppMetadata
                                {
                                    PhoneNumberId = "phone-1",
                                    DisplayPhoneNumber = "+1",
                                },
                                Statuses =
                                [
                                    new WhatsAppMessageStatus
                                    {
                                        Id = "msg-001",
                                        Status = "delivered",
                                        Timestamp = "1700000000",
                                        RecipientId = "15550001111",
                                    },
                                ],
                            },
                        },
                    ],
                },
            ],
        };

        await _sut.ProcessEventAsync(payload);

        _publisherMock.Verify(
            p => p.PublishBatchAsync(It.IsAny<IReadOnlyList<IncomingMessage>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessEventAsync_TwoChangesWithMessages_PublishesTwoBatches()
    {
        WhatsAppWebhookPayload payload = new WhatsAppWebhookPayload
        {
            Object = "whatsapp_business_account",
            Entry =
            [
                new WhatsAppEntry
                {
                    Id = "acct-1",
                    Changes =
                    [
                        BuildChange("phone-1", "+1", [("111", "msg-1", "text", "Hi")]),
                        BuildChange("phone-2", "+2", [("222", "msg-2", "text", "Hey")]),
                    ],
                },
            ],
        };

        await _sut.ProcessEventAsync(payload);

        _publisherMock.Verify(
            p => p.PublishBatchAsync(It.IsAny<IReadOnlyList<IncomingMessage>>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static WhatsAppWebhookPayload BuildPayload(
        string entryId,
        string phoneNumberId,
        string displayPhone,
        (string From, string Id, string Timestamp, string Type, string? Body)[] messages)
    {
        return new WhatsAppWebhookPayload
        {
            Object = "whatsapp_business_account",
            Entry =
            [
                new WhatsAppEntry
                {
                    Id = entryId,
                    Changes = [BuildChange(phoneNumberId, displayPhone, messages.Select(m => (m.From, m.Id, m.Type, m.Body)).ToArray())],
                },
            ],
        };
    }

    private static WhatsAppChange BuildChange(
        string phoneNumberId,
        string displayPhone,
        (string From, string Id, string Type, string? Body)[] messages)
    {
        return new WhatsAppChange
        {
            Field = "messages",
            Value = new WhatsAppChangeValue
            {
                MessagingProduct = "whatsapp",
                Metadata = new WhatsAppMetadata
                {
                    PhoneNumberId = phoneNumberId,
                    DisplayPhoneNumber = displayPhone,
                },
                Messages = messages.Select(m => new WhatsAppMessage
                {
                    From = m.From,
                    Id = m.Id,
                    Timestamp = "1700000000",
                    Type = m.Type,
                    Text = m.Body is not null ? new WhatsAppTextBody { Body = m.Body } : null,
                }).ToList(),
            },
        };
    }

    private static WhatsAppWebhookPayload BuildPayloadWithContact(string from, string messageId, string contactName)
    {
        return new WhatsAppWebhookPayload
        {
            Object = "whatsapp_business_account",
            Entry =
            [
                new WhatsAppEntry
                {
                    Id = "acct-1",
                    Changes =
                    [
                        new WhatsAppChange
                        {
                            Field = "messages",
                            Value = new WhatsAppChangeValue
                            {
                                MessagingProduct = "whatsapp",
                                Metadata = new WhatsAppMetadata
                                {
                                    PhoneNumberId = "phone-1",
                                    DisplayPhoneNumber = "+1",
                                },
                                Contacts =
                                [
                                    new WhatsAppContact
                                    {
                                        WaId = from,
                                        Profile = new WhatsAppProfile { Name = contactName },
                                    },
                                ],
                                Messages =
                                [
                                    new WhatsAppMessage
                                    {
                                        From = from,
                                        Id = messageId,
                                        Timestamp = "1700000000",
                                        Type = "text",
                                        Text = new WhatsAppTextBody { Body = "Hi" },
                                    },
                                ],
                            },
                        },
                    ],
                },
            ],
        };
    }
}
