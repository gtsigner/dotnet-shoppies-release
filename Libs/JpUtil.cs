using System;
using rv_core.Utils;

namespace JpGoods.Libs
{
    public static class JpUtil
    {
        public static readonly string TOKEN_SIGN_KEY = "tV57VyqnGxgy";
        public static readonly string APP_VERSION = "3.0.15";

        public static string GetApplicationToken(string appVersion, string uuid, string tokenSignKey = "tV57VyqnGxgy")
        {
            var signStr = $"{uuid}:{appVersion}";
            return SecureUtil.Hash(tokenSignKey, signStr);
        }

        /// <summary>
        /// 随机生成UUID
        /// </summary>
        /// <returns></returns>
        public static string GetUuid()
        {
            //00f35997
            var str = Guid.NewGuid().ToString();
//            var src = CryptUtil.Md5Encode(str).Substring(0, 8); //序列号
            var src = "00f35997";
            var androidId = CryptUtil.Md5Encode(str).Substring(0, 16); // 16个长度
            str = $"{src}_{androidId}";
            
            return SecureUtil.Sha256(str).ToLower();
        }
    }
}