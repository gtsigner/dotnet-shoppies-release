using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JpGoods.Api
{
    public class RequestBody
    {
        private G _g;
        private object _c;

        [JsonProperty("g")]
        public G G
        {
            get => _g;
            set => _g = value;
        }

        [JsonProperty("c")]
        public object C
        {
            get => _c;
            set => _c = value;
        }
    }

    public class G
    {
        private string _version = "3.0.15";
        private string _masterUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        private string _pageId = "VIE_SC004";
        private string _sessionId = "399aa7099fe5ba3db4e2d0184d54ed68";
        private string _token = "59c8b4f3d689669b4cae28a786e4ccea12c886f164c6a97b7a11e935e4f114ee";

        [JsonProperty("master_update")]
        public string MasterUpdate
        {
            get => _masterUpdate;
            set => _masterUpdate = value;
        }

        [JsonProperty("page_id")]
        public string PageId
        {
            get => _pageId;
            set => _pageId = value;
        }

        [JsonProperty("session_id")]
        public string SessionId
        {
            get => _sessionId;
            set { _sessionId = value; }
        }

        [JsonProperty("token")]
        public string Token
        {
            get => _token;
            set => _token = value;
        }

        [JsonProperty("apl_version")]
        public string Version
        {
            get => _version;
            set => _version = value;
        }
    }

    public class Methods
    {
        public static readonly string GetItemDetail = "getItemDetail";
        public static readonly string Login = "login";
        public static readonly string AutoLogin = "autoLogin";
        public static readonly string RegistUserTemp = "registUserTemp";
        public static readonly string GetBrandList = "getBrandList";
        public static readonly string GetCategoryList = "getCategoryList";
    }

    public class CItem
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("option")]
        public object Option { get; set; }
    }
}