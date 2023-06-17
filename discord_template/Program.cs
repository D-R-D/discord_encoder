using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.Configuration;

namespace discord_template
{
    class Program
    {
        public static AppSettingsReader reader = new AppSettingsReader();

        private DiscordSocketClient? _client;
        private static CommandService? _commands;
        private InteractionService? _interactionService;

        public static void Main(string[] args)
        {
            // ギルドコマンドを登録する
            CommandSender.RegisterGuildCommands();
            Console.WriteLine("CommandSender SUCCESS!!");

            _ = new Program().MainAsync();

            Thread.Sleep(-1);
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _client.Log += Log;
            _commands.Log += Log;

            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _interactionService = new InteractionService(_client.Rest);

            await _client.LoginAsync(TokenType.Bot, reader.GetValue("token", typeof(string)).ToString());
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}" + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            } else { Console.WriteLine($"[General/{message.Severity}] {message}"); }

            return Task.CompletedTask;
        }
        public async Task Client_Ready()
        {
            //クライアント立ち上げ時の処理
            await Task.CompletedTask;
        }

        //
        // そのうち消すかも
        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            //コマンド受信時の処理
            await command.RespondAsync("テスト");
        }
    }
}