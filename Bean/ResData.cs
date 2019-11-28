using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JpGoods.Bean
{
    public class ResData
    {
        [JsonProperty("show_flag")] public int ShowFlag { get; set; } = 0;

        [JsonProperty("status_code")] public int StatusCode { get; set; } = 0;

        [JsonProperty("User")] public JObject User { get; set; }
        [JsonProperty("Item")] public JObject Item { get; set; }
        [JsonProperty("Auth")] public JObject Auth { get; set; }
        [JsonProperty("Sales")] public JObject Sales { get; set; }
        [JsonProperty("master_info")] public JObject MasterInfo { get; set; }

        [JsonProperty("error")] public Error Error { get; set; }
    }

    public class Error
    {
        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("message")] public string Message { get; set; }

        [JsonProperty("layout")] public int Layout { get; set; }

        [JsonProperty("button")] public JArray Button { get; set; }
    }
}