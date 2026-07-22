using D2NG.Core;
using D2NG.Core.BNCS.Packet;
using D2NG.Core.D2GS.Packet;
using D2NG.Core.D2GS.Packet.Incoming;
using ConsoleBot.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ConsoleBot.Clients.ExternalMessagingClient;

public class ExternalMessagingClient : IExternalMessagingClient
{
    private readonly ExternalMessagingConfiguration _externalConfiguration;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly List<Client> _clients = [];
    private readonly object _clientsLock = new();
    private readonly ILogger<ExternalMessagingClient> _logger;

    public ExternalMessagingClient(IOptions<ExternalMessagingConfiguration> externalConfiguration, ILogger<ExternalMessagingClient> logger)
    {
        _externalConfiguration = externalConfiguration.Value ?? throw new ArgumentNullException(nameof(externalConfiguration), $"ExternalMessagingClient constructor fails due to {nameof(externalConfiguration)} being null");
        _telegramBotClient = new TelegramBotClient(_externalConfiguration.TelegramApiKey);
        _logger = logger;
        if(_externalConfiguration.ReceiveMessages)
        {
            _logger.LogInformation("Telegram inbound message polling enabled. Command admin id: {AdminId}, outbound chat id: {ChatId}", _externalConfiguration.TelegramAdminUserId, _externalConfiguration.TelegramChatId);
            var receiverOptions = new ReceiverOptions
            {
            };
            _telegramBotClient.StartReceiving(
                (botClient, update, token) => HandleUpdateAsync(update),
                (botClient, exception, token) => HandleExceptionAsync(exception, _logger),
                receiverOptions
            );
        }
        else
        {
            _logger.LogWarning("Telegram inbound message polling is disabled because externalMessaging.receiveMessages is false");
        }
    }

    public void RegisterClient(Client client)
    {
        lock (_clientsLock)
        {
            _clients.Add(client);
        }

        client.OnReceivedPacketEvent(Sid.CHATEVENT, (packet) => HandleChatEvent(client, packet));
        client.OnReceivedPacketEvent(InComingPacket.ReceiveChat, (packet) => HandleChatMessageEvent(client, packet));
    }

    private async Task HandleUpdateAsync(Update update)
    {
        if (update.Message is Message message)
        {
            if (message == null || message.Type != MessageType.Text)
            {
                return;
            }

            _logger.LogInformation("Text received: {Text}", message.Text);

            if (message.Text.StartsWith("/bot", StringComparison.OrdinalIgnoreCase))
            {
                await HandleBotCommandAsync(message);
                return;
            }

            Client[] clients;
            lock (_clientsLock)
            {
                clients = _clients.ToArray();
            }

            var client = clients.FirstOrDefault(c => message.Text.StartsWith(c.LoggedInUserName() + " ", StringComparison.InvariantCultureIgnoreCase));
            if (client != null)
            {
                var modifiedText = message.Text[(client.LoggedInUserName().Length + 1)..];
                if (modifiedText.StartsWith("/w", StringComparison.OrdinalIgnoreCase) || modifiedText.StartsWith("/msg", StringComparison.OrdinalIgnoreCase))
                {
                    client.Chat.Send(modifiedText);
                }
                else if (modifiedText.StartsWith("/chat", StringComparison.OrdinalIgnoreCase))
                {
                    client.Chat.Send(modifiedText[5..]);
                }
                else if (client.Game.IsInGame())
                {
                    client.Game.SendInGameMessage(modifiedText);
                }
            }
        }

        return;
    }

    private async Task HandleBotCommandAsync(Message message)
    {
        var senderUserId = message.From?.Id;
        var senderChatId = message.Chat.Id;

        if (senderUserId != _externalConfiguration.TelegramAdminUserId && senderChatId != _externalConfiguration.TelegramAdminUserId)
        {
            _logger.LogWarning("Unauthorized bot command sender. FromId={FromId}, ChatId={ChatId}", senderUserId, senderChatId);
            await SendMessage("Unauthorized bot command sender");
            return;
        }

        if (!TryParseBotCommand(message.Text, out var command))
        {
            await SendMessage("Unsupported bot command. Use /bot stop or /bot start");
            return;
        }

        switch (command)
        {
            case "stop":
                if (BotRunControl.Stop())
                {
                    await SendMessage("Bot stopped (paused at safe checkpoints)");
                }
                else
                {
                    await SendMessage("Bot is already stopped");
                }
                break;
            case "start":
                if (BotRunControl.Start())
                {
                    await SendMessage("Bot started (resumed)");
                }
                else
                {
                    await SendMessage("Bot is already running");
                }
                break;
            default:
                await SendMessage("Unsupported bot command. Use /bot stop or /bot start");
                break;
        }
    }

    private static bool TryParseBotCommand(string text, out string command)
    {
        command = string.Empty;
        var parts = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !parts[0].Equals("/bot", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (parts[1].Equals("stop", StringComparison.OrdinalIgnoreCase))
        {
            command = "stop";
            return true;
        }

        if (parts[1].Equals("start", StringComparison.OrdinalIgnoreCase))
        {
            command = "start";
            return true;
        }

        return false;
    }

    private static Task HandleExceptionAsync(Exception exception, ILogger logger)
    {
        logger.LogInformation("Exception received: {Exception}", exception);
        return Task.CompletedTask;
    }

    public async Task SendMessage(string message)
    {
        await _telegramBotClient.SendMessage(new ChatId(_externalConfiguration.TelegramChatId), message);
    }

    private void HandleChatEvent(Client client, BncsPacket obj)
    {
        var packet = new ChatEventPacket(obj.Raw);
        if (packet.Eid != Eid.SHOWUSER && packet.Eid != Eid.USERFLAGS && !packet.Username.Contains(client.LoggedInUserName()))
        {
            _logger.LogDebug("Chat event: {Text}", packet.RenderText());
            if (packet.Eid == Eid.WHISPER || packet.Eid == Eid.TALK)
            {
                SendMessage($"To {client.LoggedInUserName()} :" + packet.RenderText()).Wait();
            }
        }
    }

    private void HandleChatMessageEvent(Client client, D2gsPacket obj)
    {
        var packet = new ChatPacket(obj);
        if (packet.ChatType != 0x04)
        {
            _logger.LogDebug("Chat message: {Text}", packet.RenderText());
            if (packet.CharacterName != client.Game.Me?.Name)
            {
                SendMessage($"To {client.LoggedInUserName()} :" + packet.RenderText()).Wait();
            }
        }
    }
}
