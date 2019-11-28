using System.Collections.Generic;
using Newtonsoft.Json;

namespace JpGoods.Bean
{
    public class CarryMethod
    {
        [JsonProperty("method_id")] public int MethodId { get; set; }
    }

    public class SaleBean
    {
        [JsonProperty("atr_status")] public int AtrStatus { get; set; }
        [JsonProperty("brand_id")] public object BrandId { get; set; }
        [JsonProperty("carry_fee_type")] public int CarryFeeType { get; set; }

        [JsonProperty("carry_method")] public List<CarryMethod> CarryMethod { get; set; }
        [JsonProperty("category_id")] public string CategoryId { get; set; }
        [JsonProperty("explanation")] public string Explanation { get; set; }
        [JsonProperty("img_list")] public string ImgList { get; set; }
        [JsonProperty("input_price")] public int InputPrice { get; set; }
        [JsonProperty("item_id")] public int ItemId { get; set; }
        [JsonProperty("mode")] public int Mode { get; set; }
        [JsonProperty("no_price_flag")] public int NoPriceFlag { get; set; }
        [JsonProperty("prefecture")] public int Prefecture { get; set; }
        [JsonProperty("private_flag")] public int PrivateFlag { get; set; }
        [JsonProperty("private_member_id")] public object PrivateMemberId { get; set; }
        [JsonProperty("rot_status")] public int RotStatus { get; set; }
        [JsonProperty("send_date_standard")] public int SendDateStandard { get; set; }
        [JsonProperty("size_id")] public int SizeId { get; set; } = 7; //1,2,3,4,5,6,7 7=通用
        [JsonProperty("title")] public string Title { get; set; }
    }
}