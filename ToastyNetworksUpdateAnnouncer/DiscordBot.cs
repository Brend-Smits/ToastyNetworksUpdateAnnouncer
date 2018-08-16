using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace ToastyNetworksUpdateAnnouncer
{
    public class DiscordBot
    {
        private static DiscordSocketClient _client;
        private UpdateChecker UpdateChecker { get; set; }
        private bool IsReady { get; set; }

        static void Main(string[] args)
            => new DiscordBot().MainAsync().GetAwaiter().GetResult();

        public DiscordBot()
        {
            _client = new DiscordSocketClient();
            _client.Ready += async () => { IsReady = true; };
        }

        public async Task MainAsync()
        {
            _client.Log += Log;
            _client.MessageReceived += MessageReceived;
            await _client.LoginAsync(TokenType.Bot, "");
            await _client.StartAsync();
            await RunPeriodicAsync(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5),
                CancellationToken.None);
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            try
            {
                if (message.Channel.Id == 225347404197658624)
                {
                    if (message.Content == "/toastycheck")
                    {
                        await message.Channel.SendMessageAsync("Checking now!");
                        await GetModpackInfo();
                        await message.Channel.SendMessageAsync("Done checking!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // The `onTick` method will be called periodically unless cancelled.
        private async Task RunPeriodicAsync(
            TimeSpan dueTime,
            TimeSpan interval,
            CancellationToken token)
        {
            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);

            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                await Task.Factory.StartNew(async () => { await GetModpackInfo(); }, token);


                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, token);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }


        public async Task GetModpackInfo()
        {
            try
            {
                UpdateChecker = new UpdateChecker(this);
                Console.WriteLine("Checking for Update");
                UpdateChecker.CreateConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task Announce(string modpackName, string version, string changelogUrl, bool tagEveryone,
            ulong channelId)
        {
            try
            {
                if (IsReady)
                {
                    var chnl = _client.GetChannel(channelId) as IMessageChannel;
                    if (tagEveryone)
                    {
                        if (chnl != null)
                            await chnl.SendMessageAsync("@everyone " + "***" + modpackName + "***" +
                                                        " has been updated to version: " + version +
                                                        "! Please update as soon as possible.");
                    }
                    else
                    {
                        if (chnl != null)
                            await chnl.SendMessageAsync("***" + modpackName + "***" +
                                                        " has been updated to version: " + version +
                                                        "! Please update as soon as possible.");
                    }

                    if (chnl != null)
                        await chnl.SendMessageAsync("Interested in the changelog? Visit: " + "**" + changelogUrl +
                                                    "**");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}