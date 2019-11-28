using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using HtmlAgilityPack;
using JpGoods.Model;
using Newtonsoft.Json;

namespace JpGoods.Libs
{
    public static class JpParse
    {
        /// <summary>
        /// 解析分析
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static int ParsePagerCount(string html)
        {
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);
                var xpath = "//div[@class='pager']/nav[@class='pagerInner']/ul/li[@class='last']";
                var pager = document.DocumentNode.SelectSingleNode(xpath);
                if (pager == null)
                {
                    return 0;
                }

                var page = pager.InnerText.Replace("…", "");
                return Int32.Parse(page);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// 解析商品
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static Goods ParseGoods(string html)
        {
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);
                var pair = new Dictionary<string, string>();
                var node = document.DocumentNode;
                var goods = new Goods();


                //获取图片
                var imgs = node.SelectNodes("//div[@id='bx-pager']/a/img");
                var titleItem = node.SelectSingleNode("//h1[@class='item-title']");
                if (titleItem == null)
                {
                    return null;
                }

                var title = titleItem.InnerText;

                var brand = node.SelectSingleNode("//div[@class='item-brand']").InnerText;
                var price = node.SelectSingleNode("//div[@class='item-price']/span[@class='price']").InnerText
                    .Replace("&yen;", "").Replace(",", "");

                var desc = node.SelectSingleNode("//div[@class='descWord']").InnerText;
                var id = node.SelectSingleNode("//form[@class='sendPop']/input[@name='id']")
                    .GetAttributeValue("value", "0");


                var images = new string[imgs.Count];
                for (var i = 0; i < imgs.Count; i++)
                {
                    images[i] = imgs[i].GetAttributeValue("src", "").Replace("FE", "MA"); //都切换成大图
                }

                var settings = node.SelectNodes("//div[@class='item-settingList']/a");
                foreach (var setting in settings)
                {
                    var head = setting.SelectSingleNode("span[@class='head']").InnerText.Trim();
                    var info = setting.SelectSingleNode("span[@class='info']").InnerText.Trim();
                    pair.Add(head, info);
                    if (head.Equals("ブランド"))
                    {
                        var brandId = setting.GetAttributeValue("href", "");
                        brandId = brandId.Replace("https://shoppies.jp/user-item_list/brandid-", "");
                        goods.BrandId = brandId;
                        goods.BrandName = info;
                    }

                    if (head.Equals("カテゴリ"))
                    {
                        var categoryId = setting.GetAttributeValue("href", "");
                        categoryId = categoryId.Replace("https://shoppies.jp/user-item_list/cid-", "");
                        goods.CategoryId = categoryId;
                        goods.CategoryName = info;
                    }

                    if (head.Equals("出品地域"))
                    {
                        goods.Area = info;
                    }

                    if (head.Equals("発送日の目安"))
                    {
                        goods.ShippingDate = info;
                    }

                    if (head.Equals("配送方法"))
                    {
                        goods.ShippingMethod = info;
                    }

                    if (head.Equals("配送料"))
                    {
                        goods.ShippingLiao = info;
                    }

                    if (head.Equals("商品の状態"))
                    {
                        goods.Status = info;
                    }
                }

                goods.Title = title.Trim();
                goods.Brand = brand;
                goods.Price = Decimal.Parse(price);
                goods.GoodsNo = Int32.Parse(id);
                goods.Desc = desc.Trim();
                goods.Images = images;
                goods.ImagesString = JsonConvert.SerializeObject(images);

                return goods;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 解析商品ID
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<string> ParseGoodsIds(string html)
        {
            try
            {
                var reg = new Regex("\\/user-item\\/(\\d{2,})", RegexOptions.ECMAScript);
                var collection = reg.Matches(html);
                var list = new List<string>();
                foreach (Match o in collection)
                {
                    list.Add(o.Value.Replace("/user-item/", ""));
                }

                return list;
            }
            catch (Exception e)
            {
                return new List<string>();
            }
        }
    }
}