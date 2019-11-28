using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JpGoods.Libs
{
    public static class JpConfig
    {
        //方式
        public static readonly ObservableCollection<KeyValue> ShippingMethods = new ObservableCollection<KeyValue>
        {
        };

        /// <summary>
        /// 地区
        /// </summary>
        public static readonly ObservableCollection<KeyValue> Areas = new ObservableCollection<KeyValue>
        {
        };

        /// <summary>
        /// 地区
        /// </summary>
        public static readonly ObservableCollection<KeyValue> Categories = new ObservableCollection<KeyValue>
        {
        };

        /// <summary>
        /// 配送日期
        /// </summary>
        public static readonly ObservableCollection<KeyValue> ShippingDates = new ObservableCollection<KeyValue>
        {
        };

        /// <summary>
        /// 初始化
        /// </summary>
        static JpConfig()
        {
            var categories = JsonConvert.DeserializeObject<JObject>(Properties.Resources.categories).Properties();
            foreach (var prop in categories)
            {
                Categories.Add(new KeyValue {Title = prop.Value.ToString(), Value = prop.Name});
            }

            var areas = JsonConvert.DeserializeObject<JObject>(Properties.Resources.areas).Properties();
            foreach (var prop in areas)
            {
                Areas.Add(new KeyValue {Title = prop.Value.ToString(), Value = prop.Name});
            }

            //shipps
            var shippings = JsonConvert.DeserializeObject<JObject>(Properties.Resources.shippings).Properties();
            foreach (var prop in shippings)
            {
                ShippingMethods.Add(new KeyValue {Title = prop.Value.ToString(), Value = prop.Name});
            }
            
            //shipps
            var days = JsonConvert.DeserializeObject<JObject>(Properties.Resources.shipday).Properties();
            foreach (var prop in days)
            {
                ShippingDates.Add(new KeyValue {Title = prop.Value.ToString(), Value = prop.Name});
            }
        }
    }

    public class KeyValue
    {
        public string Title { get; set; }
        public string Value { get; set; }
    }
}