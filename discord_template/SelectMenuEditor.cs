using Discord;
using discord_template.char_controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace discord_template
{
    internal class SelectMenuEditor
    {
        public static async Task<SelectMenuBuilder> CreateEncoderMenu(int page, string CommandMode)
        {
            SelectMenuBuilder builder = new SelectMenuBuilder().WithPlaceholder($"エンコーダ一覧 p.{page}").WithCustomId($"{CommandMode}").WithMinValues(1).WithMaxValues(1);

            if (page > 0)
            {
                builder.AddOption("PreviousPage.", $"page@{page - 1}", $"Go to page {page - 1}.");
            }

            var encoders = await EncoderTaker.GetPagedEncoder(page);

            foreach (var encoder in encoders)
            {
                try 
                {
                    builder.AddOption(encoder.Name, $"{CommandMode}@{encoder.Name}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            if(await EncoderTaker.EncoderPageExist(page + 1))
            {
                builder.AddOption("Next Page.", $"page@{page + 1}", $"Go to page {page + 1}");
            }

            return builder;
        }
    }
}
