using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using HtmlAgilityPack;
using JpGoods.Bean;
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
                goods.BrandName = brand;
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

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Goods ParseBeanToGoods(ItemBean item)
        {
            var goods = new Goods();
            goods.MemberId = item.member_id;
            goods.GoodsNo = item.item_id;
            goods.ItemId = item.item_id;
            goods.BrandId = item.brand_id + "";
            goods.BrandName = item.brand;
            goods.CategoryId = item.category_id;
            goods.CategoryName = JpConfig.GetCategoryName(item.category_id);
            goods.ShippingLiao = JpConfig.GetCarryType(item.carry_fee_type + "");
            goods.ShippingMethod = JpConfig.GetShippingMethodName(item.carry_method);
            goods.ShippingArea = item.area;
            goods.ShippingDate = JpConfig.GetShippingDateName(item.send_date_standard + "");
            goods.Area = item.area;
            goods.Title = item.title;
            goods.Desc = item.explanation;
            goods.Price = item.input_price;
            goods.Images = item.picture.ConvertAll((pic) => pic.url).ToArray();
            goods.Status = JpConfig.GetStatus(item.atr_status + "");
            goods.Size = item.size_id + ""; //SIZE_ID
            goods.ImagesString = item.img_list; //图片
            return goods;
        }

        public static ItemBean ParseGoodsToItemBean(Goods goods)
        {
            var item = new ItemBean();
            var brandId = JpConfig.GetCateByTitle(JpConfig.BrandList, goods.BrandName)?.Value ?? "0";
            item.brand_id = Int32.Parse(brandId);
            item.category_id = JpConfig.GetCateByTitle(JpConfig.Categories, goods.CategoryName)?.Value ?? "0";

            //配送
            var liao = JpConfig.GetCateByTitle(JpConfig.ShippingType, goods.ShippingLiao)?.Value ?? "0";
            item.carry_fee_type = Int32.Parse(liao);
            item.carry_method = JpConfig.GetCateByTitle(JpConfig.ShippingMethods, goods.ShippingMethod)?.Value ?? "0";

            //日期
            var day = JpConfig.GetCateByTitle(JpConfig.ShippingDates, goods.ShippingMethod)?.Value ?? "0";
            item.send_date_standard = Int32.Parse(day);
            item.area = goods.Area;
            item.size_id = Int32.Parse(goods.Size);
            item.title = goods.Title;
            item.explanation = goods.Desc;
            item.no_price_flag = 0;
            item.rot_status = 2;
            item.input_price = goods.Price;
            item.private_member_id = 0;
            item.private_flag = 0;


            return item;
        }


        public static SaleBean ParseGoodsToSaleBean(Goods goods)
        {
            var item = new SaleBean();
            var brandId = JpConfig.GetCateByTitle(JpConfig.BrandList, goods.BrandName)?.Value ?? "0";
            item.BrandId = Int32.Parse(brandId);
            item.CategoryId = JpConfig.GetCateByTitle(JpConfig.Categories, goods.CategoryName)?.Value ?? "0";


            //付款方式
            var method = Int32.Parse(JpConfig.GetCateByTitle(JpConfig.ShippingMethods, goods.ShippingMethod)?.Value ??
                                     "0");
            item.CarryMethod = new List<CarryMethod>
            {
                new CarryMethod {MethodId = method}
            };

            //配送
            var lia = JpConfig.GetCateByTitle(JpConfig.ShippingType, goods.ShippingLiao)?.Value ?? "0";
            item.CarryFeeType = method <= 10 ? 0 : 1; //和ShippingMethod对应

            //日期
            var day = JpConfig.GetCateByTitle(JpConfig.ShippingDates, goods.ShippingMethod)?.Value ?? "0";
            item.SendDateStandard = Int32.Parse(day);

            //区域
            var areaId = JpConfig.GetCateByTitle(JpConfig.Areas, goods.Area)?.Value ?? "0";
            item.Prefecture = Int32.Parse(areaId);

            item.SizeId = Int32.Parse(goods.Size);
            item.Title = goods.Title;
            item.Explanation = goods.Desc;
            item.NoPriceFlag = 0; //1没价格，
            item.RotStatus = 2;
            item.InputPrice = goods.Price;
            item.PrivateMemberId = null;
            item.PrivateFlag = 0;
            item.ItemId = goods.ItemId; //如果是创建就指0
            var statId = JpConfig.GetCateByTitle(JpConfig.StatusType, goods.Status)?.Value ?? "0";
            item.AtrStatus = Int32.Parse(statId);
            item.Mode = 2; //?? 1=预览,2=发布
            item.RotStatus = 2; //默认

            return item;
        }
    }
}