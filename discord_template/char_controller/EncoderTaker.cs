using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace discord_template.char_controller
{
    public class EncoderTaker
    {
        public static async Task<List<EncodingInfo>> GetPagedEncoder(int page)
        {
            while(Settings.Shared == null)
            {
                await Task.Yield();
            }

            return Settings.Shared.m_EncoderList.Skip(page * 16).Take(16).ToList();
        }

        public static async Task<bool> EncoderPageExist(int page)
        {
            while(Settings.Shared == null)
            {
                await Task.Yield();
            }

            return Settings.Shared.m_EncoderList.Count > page * 16;
        }
    }
}
