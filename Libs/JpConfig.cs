using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

        //配送料 carry_fee_type
        public static readonly ObservableCollection<KeyValue> ShippingType = new ObservableCollection<KeyValue>
        {
            new KeyValue {Title = "送料別（購入者が払う）", Value = "0"},
            new KeyValue {Title = "送料込み（出品者が払う）", Value = "1"}
        };

        //商品状态 atr_status
        public static readonly ObservableCollection<KeyValue> StatusType = new ObservableCollection<KeyValue>
        {
            new KeyValue {Title = "新品、未使用", Value = "1"},
            new KeyValue {Title = "未使用に近い", Value = "2"},
            new KeyValue {Title = "目立った傷や汚れなし", Value = "3"},
            new KeyValue {Title = "やや傷や汚れあり", Value = "4"},
            new KeyValue {Title = "傷や汚れあり", Value = "5"},
            new KeyValue {Title = "全面的に状態が悪い", Value = "6"},
        };

        /// <summary>
        /// 品牌列表
        /// </summary>
        public static readonly ObservableCollection<KeyValue> BrandList = new ObservableCollection<KeyValue>
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


        #region 获取名称

        public static string GetCategoryName(string categoryNo)
        {
            var cate = Categories.First(c => c.Value == categoryNo);
            return cate != null ? cate.Title : "";
        }

        public static string GetShippingMethodName(string no)
        {
            var cate = ShippingMethods.First(c => c.Value == no);
            return cate != null ? cate.Title : "";
        }

        public static string GetAreaName(string no)
        {
            var cate = Areas.First(c => c.Value == no);
            return cate != null ? cate.Title : "";
        }

        public static string GetShippingDateName(string no)
        {
            var cate = ShippingDates.First(c => c.Value == no);
            return cate != null ? cate.Title : "";
        }

        public static string GetStatus(string no)
        {
            var cate = StatusType.First(c => c.Value == no);
            return cate != null ? cate.Title : "";
        }

        public static string GetCarryType(string no)
        {
            var cate = ShippingType.First(c => c.Value == no);
            return cate != null ? cate.Title : "";
        }

        #endregion

        #region 通过名称获取对象

        /// <summary>
        /// 筛选
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static KeyValue GetCateByTitle(Collection<KeyValue> coll, string title)
        {
            try
            {
                var cate = coll.First(c => c.Title == title);
                return cate;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static KeyValue GetCateByValue(Collection<KeyValue> coll, string value)
        {
            try
            {
                var cate = coll.First(c => c.Value == value);
                return cate;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        #endregion
    }

    public class KeyValue
    {
        public string Title { get; set; }
        public string Value { get; set; }

        public object Options { get; set; }
    }
}