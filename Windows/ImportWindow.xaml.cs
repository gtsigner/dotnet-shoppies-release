using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using JpGoods.Bean;
using JpGoods.Libs;
using JpGoods.Model;
using JpGoods.Windows.Import;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JpGoods.Windows
{
    public partial class ImportWindow : Window
    {
        private readonly MainWindow _mainWindow;
        private readonly ImportContext _context = new ImportContext();
        private readonly ObservableCollection<Goods> _goodses = new ObservableCollection<Goods>();

        public ImportWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            DataContext = _context;
            if (false == _mainWindow.Anonymous.IsLogin)
            {
                MessageBox.Show("当前匿名用户没有登录，可能会导致解析失败");
            }
        }


        /// <summary>
        /// 點擊
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            if (btn.Name.Equals(BtnParse.Name))
            {
                //解析出ID
                StartParse();
            }

            //保存
            if (btn.Name.Equals(BtnImport.Name))
            {
                _context.IsImporting = true;
                try
                {
                    //正在保存
                    foreach (var goods in _goodses)
                    {
                        var sq = from s in App.DbCtx.Goods where s.GoodsNo == goods.GoodsNo select s;
                        var ext = await sq.FirstOrDefaultAsync();
                        if (ext == null)
                        {
                            await App.DbCtx.Goods.AddAsync(goods);
                        }
                        else
                        {
                            //App.DbCtx.Entry(goods).CurrentValues.SetValues(goods);
                        }
                    }

                    await App.DbCtx.SaveChangesAsync();
                    _context.LogText = $"保存成功：{_goodses.Count}";
                }
                catch (Exception ex)
                {
                    //
                    Debug.WriteLine(ex);
                }

                _context.IsImporting = false;
            }
        }

//        private async void StartParse()
//        {
//            _context.IsParsing = true;
//            _goodses.Clear(); //清空
//            _context.GoodsCount = 0;
//
//            _context.LogText = $"正在解析总页数:{_context.Url}";
//            var res = await _mainWindow.Api.ViewUrl(_context.Url);
//            if (!res.Ok)
//            {
//                _context.LogText = $"解析失败:{res.Message} / {res.Status}";
//                return;
//            }
//
//            //1.解析分页
//            var page = JpParse.ParsePagerCount(res.Data.ToString());
//            _context.LogText = $"一共解析出：{page} 页";
//            if (page > 0)
//            {
//                var userShopId = _context.Url.Replace("https://shoppies.jp/user-shop/", "");
//
//                //2.解析具体的分页内容
//                for (var i = 0; i < page; i++)
//                {
//                    var p = i + 1;
//                    await ParsePage(userShopId, p);
//                }
//            }
//            else
//            {
//                _context.LogText = $"内容：{res.Data}";
//            }
//
//            _context.IsParsing = false;
//        }
        private async void StartParse()
        {
            _context.IsParsing = true;
            _goodses.Clear(); //清空
            _context.GoodsCount = 0;
            try
            {
                _context.LogText = $"正在解析总页数:{_context.Url}";
                //解析url 识别shopId
                var match = Regex.Match(_context.Url, "\\d+$");
                var shopId = match?.Value ?? "";
                if (shopId.Equals(string.Empty))
                {
                    throw new Exception("解析URL ShopID失败");
                }

                var res = await _mainWindow.Api.GetShopItemList(_mainWindow.Anonymous, shopId);
                if (!res.Ok)
                {
                    throw new Exception($"解析失败:{res.Message} / {res.Status}");
                }

                var data = res.Data as ResData;
                if (null == data || data?.StatusCode != 1)
                {
                    throw new Exception($"获取失败:{data?.Error.Message}");
                }

                var beans = data.Item.GetValue("getShopItemList").ToObject<JObject>().GetValue("list")
                    .ToObject<List<ItemBean>>();

                var ids = beans.ConvertAll((bean) => bean.item_id.ToString());
                _context.LogText = $"获取：{ids.Count}个商品";
                //解析
                var task = ParseGoods(ids);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                _context.LogText = $"解析失败:{e.Message}";
            }

            _context.IsParsing = false;
        }

        /// <summary>
        /// 获取商品
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task ParseGoods(List<string> ids)
        {
            try
            {
                var strs = ids.ToArray();
                var res = await _mainWindow.Api.GetItemsDetail(_mainWindow.Anonymous, strs);
                if (!res.Ok)
                {
                    throw new Exception(res.Message);
                }

                var data = res.Data as ResData;
                if (data == null)
                {
                    throw new Exception(res.Message);
                }

                if (data.StatusCode != 1)
                {
                    throw new Exception(data.Error.Message);
                }

                var items = data.Item.GetValue("getItemDetail").ToObject<JObject>().GetValue("detail_list")
                    .ToObject<List<ItemBean>>();
                _context.LogText = $"解析到：{items.Count}个商品数据，正在下载商品图片";
                //downloadImages
                foreach (var item in items)
                {
                    try
                    {
                        var urls = item.picture.ConvertAll((pic) => pic.url).ToArray();
                        var savePath = Path.Combine("Goods", item.item_id + "");
                        await DownLoadPics(savePath, urls);
                        //转化成商品后进行存库
                        //写入到config文件
                        var configFile = Path.Combine("Goods", item.item_id + "", "config.json");
                        SaveConfigFile(configFile, JsonConvert.SerializeObject(item));
                        var goods = JpParse.ParseBeanToGoods(item);
                        _goodses.Add(goods);
                        _context.GoodsCount++;
                        _context.LogText = $"商品详情:{item.item_id} 解析成功";
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        _context.LogText = $"商品详情:{item.item_id} 解析失败：{ex.Message}";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _context.LogText = $"获取goods明细失败:{e.Message} 请重试";
            }
        }

        private async void SaveConfigFile(string file, string data)
        {
            try
            {
                File.WriteAllText(file, data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="path"></param>
        /// <param name="urls"></param>
        /// <returns></returns>
        private async Task DownLoadPics(string path, string[] urls)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //解析图片末尾的地址
            for (var i = 0; i < urls.Length; i++)
            {
                var url = urls[i];
                var file = Path.Combine(path, $"{i}.jpg");
                try
                {
                    //判断是否可以进行读写,TODO 可以进行并发下载，不会封IP
                    var res = await _mainWindow.Api.GetImage(url);
                    if (!res.Ok) throw new Exception("下载图片失败");
                    //写入文件
                    var bytes = res.Data as byte[];
                    File.WriteAllBytes(file, bytes);
                    _context.LogText = $"图片：{url} 下载成功";
                }
                catch (Exception ex)
                {
                    _context.LogText = $"写入失败:{ex.Message}";
                }
            }
        }
    }
}