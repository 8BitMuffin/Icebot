using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.IO;

namespace IceBot
{
    public class IceBot
    {
        private string token = Environment.GetEnvironmentVariable("ICEBOT_TOKEN", EnvironmentVariableTarget.User);
        DiscordClient client;
        CommandService commands;

        public IceBot()
        {
            client = new DiscordClient(input =>
            {
                input.LogLevel = LogSeverity.Info;
                input.LogHandler = Log;
            });

            client.UsingCommands(input =>
            {
                input.PrefixChar = '!';
                input.AllowMentionPrefix = true;
            });
            commands = client.GetService<CommandService>();
            commands.CreateCommand("test").Do(async (e) =>
            {
                await e.Channel.SendMessage("Icebot Online!");
            });

            commands.CreateCommand("legion").Description("Params: lvlinfo <lvl>, stats <nickname>").Parameter("legionparams", ParameterType.Multiple).Do(async (e) =>
            {

                var parameters = e.Args;
                string message = "";
                message = await GetLegionInfo(parameters);

                await e.Channel.SendMessage(message);

            });

            commands.CreateCommand("help").Do(async (e) =>
            {
                string message = "";
                StringBuilder commandList = new StringBuilder();
                foreach (var command in commands.AllCommands)
                {
                    if (!String.IsNullOrWhiteSpace(command.Description))
                    {
                        commandList.AppendLine($"!{command.Text}\t{command.Description}");
                    }
                    commandList.AppendLine("!" + command.Text);
                }
                
                message = $"```#Icebot Help\n{commandList}```";
                await e.Channel.SendMessage(message);
            });

            commands.CreateCommand("toucan").Do(async (e) =>
            {
                var path = Path.GetFullPath(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..//.." + "//Copypasta//Toucan.txt"));
                if (File.Exists(path))
                {
                    string message = "";
                    using (TextReader reader = File.OpenText(path))
                    {
                        string line = "";
                        while ((line = reader.ReadLine()) != null)
                        {
                            message += line;
                        }
                    }
                    await e.Channel.SendMessage(message);
                }
                else
                {
                    await e.Channel.SendMessage("Toucan is busy");
                }

            });


            client.UserJoined += async (s, e) =>
            {
                var channel = e.Server.FindChannels("wc3", ChannelType.Voice).FirstOrDefault();
                var user = e.User;
                await channel.SendTTSMessage($"{user.Name}has joined the channel");
            };

            client.UserJoined += async (s, e) =>
            {
                var channel = e.Server.FindChannels("wc3", ChannelType.Voice).FirstOrDefault();
                var user = e.User;
                await channel.SendTTSMessage($"{user.Name}has left the channel");
            };
            
            client.ExecuteAndWait(async () =>
            {
                await client.Connect(token, TokenType.Bot);
            });
        }

        private async Task<string> GetLegionInfo(string[] parameters)
        {
            string[] commands = LegionTDService.AllowedCommands;
            string command = commands.FirstOrDefault(c => c == parameters[0].ToLower());
            if (command != null && parameters.Count() == 2)
            {

                LegionTDService ltdService = new LegionTDService(command, parameters[1]);
                var result = await ltdService.GetData();
                return "```" + result[0] + result[1] + "```";

            }

            return "No such command exists"; 
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
