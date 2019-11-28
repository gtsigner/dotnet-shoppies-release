using System.Collections.Generic;

namespace JpGoods.Bean
{
    public class Picture
    {
        public string url { get; set; }
        public int img_width { get; set; }
        public int img_height { get; set; }
        public string item_id { get; set; }
    }

    public class Brand
    {
        public int brand_id { get; set; }
        public string name { get; set; }
        public string alpha_name { get; set; }
        public string first_character { get; set; }
    }

    /// <summary>
    /// 最终会写入到配置文件
    /// </summary>
    public class ItemBean
    {
        public int member_id { get; set; }
        public int item_id { get; set; }
        public int item_status_id { get; set; }
        public string title { get; set; }
        public string title_noemoji { get; set; }
        public string explanation { get; set; }
        public List<Picture> picture { get; set; }
        public decimal input_price { get; set; }
        public string category_id { get; set; }
        public int brand_id { get; set; }
        public string brand { get; set; }
        public int size_id { get; set; }
        public int atr_status { get; set; }
        public int carry_fee_type { get; set; }
        public string carry_method { get; set; }
        public string area { get; set; }
        public int send_date_standard { get; set; }
        public int rot_status { get; set; }
        public int like_count { get; set; }
        public int like_open_flag { get; set; }
        public int like_flag { get; set; }
        public int qa_count { get; set; }
        public int private_flag { get; set; }
        public int private_member_flag { get; set; }
        public int private_member_id { get; set; }
        public string private_item_limit_date { get; set; }
        public string private_item_member_url { get; set; }
        public string private_item_shop_name { get; set; }
        public string private_item_nick_name { get; set; }
        public int order_stop_flag { get; set; }
        public string shop_information { get; set; }
        public string item_url { get; set; }
        public string registration_date { get; set; }
        public int no_price_flag { get; set; }
        public string img_list { get; set; }
        public int disp_type { get; set; }
    }
}