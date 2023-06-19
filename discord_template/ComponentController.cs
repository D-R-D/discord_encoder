using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace discord_template
{
    public class ComponentController
    {
        private SocketMessageComponent _component;

        public ComponentController(SocketMessageComponent component)
        {
            if (component == null) { throw new ArgumentNullException(nameof(component)); }
            _component = component;
        }

        public async Task<(string? label, ComponentBuilder? builder)> BuileComponent()
        {
            string CustomID = _component.Data.CustomId; // コマンド名
            string[] CustomValue = _component.Data.Values.First().Split('@');

            string CommandMode = CustomID;
            string InnerCommandName = CustomValue.First();
            string InnerCommandValue = CustomValue.Last();

            //
            // 文字列, バイトコードの相互変換
            if (CommandMode == "encode" || CommandMode == "decode")
            {
                if (InnerCommandName == "page")
                {
                    var menuBuilder = await SelectMenuEditor.CreateEncoderMenu(int.Parse(InnerCommandValue), CommandMode);
                    var builder = new ComponentBuilder().WithSelectMenu(menuBuilder);

                    return ($"[/{CommandMode}](p.{InnerCommandValue})\n以下の選択肢からエンジンを選択してください", builder);
                }

                if (InnerCommandName == "encode" || InnerCommandName == "decode")
                {
                    return (InnerCommandName, null);
                }

            }

            return (null, null);
        }
    }
}
