using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using discord_template.char_controller;
using System.Configuration;
using System.Drawing.Imaging;
using System.Text;

namespace discord_template
{
    class Program
    {
        public static AppSettingsReader reader = new AppSettingsReader();

        private static DiscordSocketClient? _client;
        private static CommandService? _commands;

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
            _client.Log += Log;
            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.SelectMenuExecuted += SelectMenuHandler;
            _client.ModalSubmitted += ModalHandler;

            _commands = new CommandService();
            _commands.Log += Log;

            await _client.LoginAsync(TokenType.Bot, reader.GetValue("token", typeof(string)).ToString());
            await _client.StartAsync();

            // Block this task until the program is closed.
            while (true)
            {
                await Task.Yield();
            }
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
            _ = Task.Run(async () =>
            {
                try
                {
                    ulong guildid = command.GuildId!.Value;
                    SelectMenuBuilder? menuBuilder = null;
                    ComponentBuilder? builder = null;
                    string commandname = command.Data.Options.First().Value.ToString()!;

                    if (!command.GuildId.HasValue)
                    {
                        await command.RespondAsync("ごめんね、guild専用なんだ");
                        return;
                    }

                    //
                    // エンコード・デコード処理
                    if (commandname == "encode" || commandname == "decode")
                    {
                        await command.DeferAsync(ephemeral: true);
                        string message = $"[/{commandname}]@(p.0)\n以下の選択肢からエンコーダを選択してください。";
                        menuBuilder = await SelectMenuEditor.CreateEncoderMenu(0, commandname);
                        builder = new ComponentBuilder().WithSelectMenu(menuBuilder);

                        await command.ModifyOriginalResponseAsync(m => 
                        {
                            m.Content = message;
                            m.Components = builder.Build();
                        });

                        return;
                    }

                    throw new Exception($"指定されたコマンド[{commandname}]は存在しません");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    if (command.HasResponded)
                    {
                        await command.ModifyOriginalResponseAsync(m => { m.Content = ex.Message; });
                        return;
                    }
                    await command.RespondAsync(ex.Message);
                }
            });

            await Task.CompletedTask;
        }

        //
        // セレクトメニューのイベント処理
        private static async Task SelectMenuHandler(SocketMessageComponent arg)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    string CustomID         = arg.Data.CustomId;          // コマンド名
                    string[] CustomValue    = arg.Data.Values.First().Split('@'); // 内部コマンド名 @ コマンド値

                    string commandName          = CustomID;
                    string InnerCommandName     = CustomValue.First();  // encode | decode
                    string InnerCommandValue    = CustomValue.Last();
                    
                    ComponentController selectMenuController = new ComponentController(arg);
                    var respondcontent = await selectMenuController.BuileComponent();

                    if(respondcontent.label == "encode" || respondcontent.label == "decode")
                    {
                        var splitter = new TextInputBuilder().WithLabel("SPLIT CHAR").WithCustomId("SPLITTER").WithStyle(TextInputStyle.Short).WithMaxLength(1).WithValue("-").WithRequired(true).WithPlaceholder("バイトコードの区切り文字を指定");
                        var textItem = new TextInputBuilder().WithLabel("INPUT TEXT").WithCustomId("VALUE").WithStyle(TextInputStyle.Paragraph).WithRequired(true).WithPlaceholder("文字列を入力");
                        var builder = new ModalBuilder().WithTitle("ENCODE").WithCustomId($"{InnerCommandName}@{InnerCommandValue}").AddTextInput(splitter).AddTextInput(textItem);

                        await arg.RespondWithModalAsync(builder.Build());
                        return;
                    }

                    //
                    // ページ選択
                    await arg.RespondAsync(respondcontent.label, components: respondcontent.builder.Build(), ephemeral: true);
                    return;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    if (arg.HasResponded)
                    {
                        await arg.ModifyOriginalResponseAsync(m => { m.Content = ex.Message; });
                        return;
                    }
                    await arg.RespondAsync(ex.Message);
                }
            });
            await Task.CompletedTask;
        }

        //
        // モーダルのイベント処理
        private static async Task ModalHandler(SocketModal modal)
        {
            _ = Task.Run(async () =>
            {
                if (modal.GuildId == null)
                {
                    await modal.RespondAsync("不正なコマンドが実行されました。");
                    return;
                }
                await modal.RespondAsync("PROCESSING...");

                List<SocketMessageComponentData> components = modal.Data.Components.ToList();
                var CustomID    = modal.Data.CustomId.Split('@');
                var command     = CustomID[0];
                var commandVal  = CustomID[1];

                try
                {
                    string result = string.Empty;
                    string splitCode = components.First(_ => _.CustomId == "SPLITTER").Value;
                    string value = components.First(_ => _.CustomId == "VALUE").Value;

                    switch (command)
                    {
                        case "encode":
                            result = strEncoder.Encode(value, commandVal, splitCode.ToCharArray()[0]);
                            break;

                        case "decode":
                            result = strDecoder.Decode(value, commandVal, splitCode.ToCharArray()[0]);
                            break;

                        default:
                            throw new Exception($"[{command}]は不明なコマンドです。");
                    }

                    if (result.Length <= 2000)
                    {
                        await modal.ModifyOriginalResponseAsync(m => { m.Content = result; });
                        return;
                    }

                    Optional<IEnumerable<FileAttachment>> optional = new();
                    using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                    {
                        FileAttachment fa = new FileAttachment(stream, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt");
                        List<FileAttachment> flis = new() { fa };
                        optional = new(flis);

                        result = $"出力文字数が上限に達しました。添付ファイルに出力を記載します。";

                        await modal.ModifyOriginalResponseAsync(m =>
                        {
                            m.Content = result;
                            m.Attachments = optional;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    await modal.ModifyOriginalResponseAsync(m => { m.Content = ex.Message; });
                }
            });

            await Task.CompletedTask;
        }
    }
}