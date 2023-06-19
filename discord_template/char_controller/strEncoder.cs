using System.Text;

namespace discord_template.char_controller
{
    internal class strEncoder
    {
        public static string Encode(string character, string encodername, char splitCode = '-', int bytes = 16)
        {
            try
            {
                string result = string.Empty;
                // エンコーダ名からエンコーダを取得
                var encoder = Encoding.GetEncoding(encodername);
                // 取得したエンコーダで文字列をバイトコードに変換
                byte[] rawByteCode = encoder.GetBytes(character);

                // バイトコードを文字列に変換
                foreach (var item in rawByteCode)
                {
                    result += Convert.ToString(int.Parse(item.ToString()), bytes) + splitCode;
                }
                return result.TrimEnd(splitCode);
            }
            catch
            {
                throw;
            }
        }
    }
}
