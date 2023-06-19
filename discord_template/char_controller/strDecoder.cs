using System.Text;

namespace discord_template.char_controller
{
    internal class strDecoder
    {
        public static string Decode(string character, string encodername, char splitCode = '-', int bytes = 16)
        {
            try
            {
                // エンコーダ名からエンコーダを取得
                var encoder = Encoding.GetEncoding(encodername);
                // 文字列化されたバイトコードを配列に直す
                string[] splitedCode = character.Split(splitCode);
                byte[] byteCode = new byte[splitedCode.Length];

                // バイトコードを指定されたエンコーダで文字列に変換する
                for (int i = 0; i < splitedCode.Length; i++)
                {
                    byteCode[i] = Convert.ToByte(splitedCode[i], bytes);
                }

                return encoder.GetString(byteCode);
            }
            catch
            {
                throw;
            }
        }
    }
}
