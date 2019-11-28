using System.Security.Cryptography;
using System.Text;
using rv_core.Utils;

namespace JpGoods.Libs
{
    public static class SecureUtil
    {
        public static string Hash(string key, string str)
        {
            return CryptUtil.HmacSHA256(str, key);
        }

        public static string Sha256(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var hash = SHA256.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("X2"));
            }

            return builder.ToString();
        }
    }
}