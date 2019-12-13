using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JpGoods.Bean;
using JpGoods.Ctx;
using JpGoods.Libs;
using JpGoods.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using rv_core.Inters;
using rv_core.Utils;
using rvcore.Model;
using rvcore.Proxy;

namespace JpGoods.Api
{
    public class ReqEvents
    {
        [JsonProperty("page")] public string Page { get; set; } = "";

        [JsonProperty("param")] public string Param { get; set; } = null;

        [JsonProperty("title")] public string Title { get; set; } = null;

        [JsonProperty("type")] public string Type { get; set; } = "view";

        [JsonProperty("time")] public long Time { get; set; } = Logger.GetTimeStampInt();
    }

    public class JpApi : IHttpRequest
    {
        private readonly IHttpClientFactory _factory;
        private readonly IProxy _proxy = null;
        private HttpProxy config;
        private readonly List<ReqEvents> _events = new List<ReqEvents>();

        public bool EnableProxy { get; set; } = false;
        public MainContext Context = null;

        public JpApi(IHttpClientFactory factory, IProxy proxy = null)
        {
            _factory = factory;
            _proxy = proxy;
        }


        /// <summary>
        /// 每10s进行以此trace
        /// </summary>
        /// <returns></returns>
        public async void SetTrackingLog(User user)
        {
            var list = _events.ToArray();
            _events.Clear();
            //转化

            var g = new G
            {
                PageId = null,
                MasterUpdate = user.MasterUpdate,
                Token = user.Token,
                SessionId = user.SessionId,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody { G = g };
            var c = new CItem
            {
                Method = Methods.SetTrackingLog,
                Option = list
            };
            var items = new List<CItem> { c };
            var cc = new Dictionary<string, object> { { "Management", items } };
            body.C = cc;
            var res = await ApiRequest(user, body);
        }


        public async Task<HttpRes> Login(User user)
        {
            var g = new G
            {
                PageId = "VIE_SC004",
                MasterUpdate = user.MasterUpdate,
                Token = user.Token,
                SessionId = user.SessionId,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody { G = g };
            var cc = new Dictionary<string, object>();
            var c = new CItem
            {
                Method = Methods.Login,
                Option = new JObject { { "mail_address", user.Username }, { "password", user.Password }, { "uuid", user.Uuid } }
            };
            var list = new List<CItem> { c };

            cc.Add("Auth", list);
            body.C = cc;
            return await ApiRequest(user, body);
        }

        /// <summary>
        /// 自动登录
        /// </summary>
        /// <returns></returns>
        public async Task<HttpRes> AutoLogin(User user)
        {
            var g = new G
            {
                PageId = "SLI_MR001",
                MasterUpdate = user.MasterUpdate,
                Token = user.Token,
                SessionId = user.SessionId,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody { G = g };
            var cc = new Dictionary<string, object>();
            var c = new CItem
            {
                Method = Methods.AutoLogin,
                Option = new JObject { { "uuid", user.Uuid } }
            };
            var list = new List<CItem> { c };
            cc.Add("Auth", list);
            body.C = cc;
            return await ApiRequest(user, body);
        }

        //注册临时用户
        public async Task<HttpRes> RegisterUserTemp(User user)
        {
            var g = new G
            {
                PageId = "SLI_MR001",
                MasterUpdate = user.MasterUpdate,
                Token = user.Token,
                SessionId = null,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody { G = g };
            var cc = new Dictionary<string, object>();
            var c = new CItem
            {
                Method = Methods.RegistUserTemp,
                Option = new JObject { { "uuid", user.Uuid } }
            };
            var list = new List<CItem> { c };
            cc.Add("User", list);
            body.C = cc;
            var res = await ApiRequest(user, body);
            if (res.Ok)
            {
                //处理
            }

            return res;
        }

        /// <summary>
        /// 获取品牌列表
        /// </summary>
        /// <returns></returns>
        public async Task<HttpRes> GetShopItemList(User user, string shopId, int limit = 1000)
        {
            var g = new G
            {
                PageId = "MYM_SH001",
                SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate,
                Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody { G = g };
            var cc = new Dictionary<string, object>();
            var opts = new Dictionary<string, object>
            {
                {"member_id", shopId},
                {"offset", 0},
                {"limit", limit},
            };
            var list = new List<CItem>
            {
                new CItem
                {
                    Method = Methods.GetShopItemList,
                    Option = opts
                },
            };
            cc.Add("Item", list);
            body.C = cc;
            return await ApiRequest(user, body);
        }

        /// <summary>
        /// 获取品牌列表
        /// </summary>
        /// <returns></returns>
        public async Task<HttpRes> GetBrandList(User user)
        {
            var g = new G
            {
                PageId = "SEL_EX017",
                SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate,
                Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody { G = g };
            var cc = new Dictionary<string, object>();
            var list = new List<CItem>
            {
                new CItem
                {
                    Method = Methods.GetBrandList,
                    Option = null
                },
            };
            cc.Add("MasterInfo", list);
            body.C = cc;
            return await ApiRequest(user, body);
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="user"></param>
        /// <param name="filename"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task<HttpRes> UploadImage(User user, string filename, byte[] bytes)
        {
            var client = ClientFactory();
            client.Timeout = TimeSpan.FromSeconds(30);
            var req = wrapperUploadImageMessage(user, filename, bytes);
            var res = await Request(client, req);
            if (res.Code == 6 || res.Status > 500)
            {
                //错误,切换IP
                await ChangeProxy();
                client = ClientFactory();
                client.Timeout = TimeSpan.FromSeconds(30);
                req = wrapperUploadImageMessage(user, filename, bytes);
                res = await Request(client, req);

            }
            return res;
        }

        /// <summary>
        /// wapper
        /// </summary>
        /// <param name="user"></param>
        /// <param name="filename"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private HttpRequestMessage wrapperUploadImageMessage(User user, string filename, byte[] bytes)
        {
            var g = new G
            {
                MasterUpdate = user.MasterUpdate,
                PageId = "SEL_EX010",
                Token = user.Token,
                Version = JpUtil.APP_VERSION,
                SessionId = user.SessionId
            };

            var body = new RequestBody { G = g };
            var c = new Dictionary<string, object>();
            var cItems = new List<CItem> { new CItem { Method = "setPhoto", Option = null } };
            c.Add("Sales", cItems);

            body.C = c;


            var content = new MultipartFormDataContent
            {
                {new StringContent(JsonConvert.SerializeObject(body.G)), "g"},
                {new StringContent(JsonConvert.SerializeObject(body.C)), "c"},
                {new ByteArrayContent(bytes), "image", filename} //最后一个是图片名称
            };

            var req = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.shoppies.jp/api/app/v01"),
                Content = content,
                Headers =
                {
                    {
                        "User-Agent",
                        "Dalvik/2.1.0 (Linux; U; Android 5.1.1; xiaomi 6 Build/LYZ28N) ShoppiesForAndroidApp Version/3.0.15"
                    }
                }
            };

            pushToQueue(user, body);
            return req;
        }


        //22424eb8161637a042548c1db545a2b2
        //uuid= 816984a5a6984b3ed8ee0f07bc89636f005dc72cb0e341d11d21b1eb6710580f
        //token=f89c17721a5b1557ec12945656a8ad2fb780be5c5ad910293369181612411d64
        public void DownGoods()
        {
        }

        public void UpGoods()
        {
        }

        /// <summary>
        /// 添加商品或者修改商品
        /// </summary>
        /// <param name="user"></param>
        /// <param name="bean"></param>
        /// <returns></returns>
        public async Task<HttpRes> SetItem(User user, SaleBean bean)
        {
            var g = new G
            {
                PageId = "SEL_EX001",
                SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate,
                Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody { G = g };
            var cc = new Dictionary<string, object>();
            var list = new List<CItem>
            {
                new CItem
                {
                    Method = Methods.SetItem2,
                    Option = bean
                },
            };
            cc.Add("Sales", list);
            body.C = cc;
            return await ApiRequest(user, body);
        }

        /// <summary>
        /// 在发布完成后校验发布结果
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<HttpRes> CheckSetItem(User user)
        {
            var g = new G
            {
                PageId = "SEL_EX001",
                SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate,
                Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody { G = g };
            var cc = new Dictionary<string, object>();
            var list = new List<CItem>
            {
                new CItem
                {
                    Method = Methods.CheckSetItem,
                    Option = null
                },
                new CItem
                {
                    Method = Methods.GetAfterItem,
                    Option = null
                },
                new CItem
                {
                    Method = Methods.GetSalesItemList,
                    Option = new JObject {{"offset", 0}}
                },
            };
            var userList = new List<CItem>
            {
                new CItem
                {
                    Method = Methods.GetAddress,
                    Option = null
                }
            };
            cc.Add("Sales", list);
            cc.Add("User", userList);
            body.C = cc;
            return await ApiRequest(user, body);
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="user"></param>
        /// <param name="beans"></param>
        /// <returns></returns>
        public async Task<HttpRes> SetItems(User user, SaleBean[] beans)
        {
            var g = new G
            {
                PageId = "SEL_EX001",
                SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate,
                Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody { G = g };
            var cc = new Dictionary<string, object>();
            var list = new List<CItem> { };
            foreach (var saleBean in beans)
            {
                var itm = new CItem
                {
                    Method = Methods.SetItem2,
                    Option = saleBean
                };
                list.Add(itm);
            }

            cc.Add("Sales", list);
            body.C = cc;
            return await ApiRequest(user, body);
        }

        public void Logout()
        {
        }

        public async Task<HttpRes> ViewUrl(string url)
        {
            var httpRes = new HttpRes { Ok = false, Data = "", Url = url };
            try
            {
                var client = _factory.CreateClient("shoppies.jp");
                client.Timeout = TimeSpan.FromSeconds(60);

                var res = await client.GetAsync(new Uri(url));
                //https://shoppies.jp/user-item/165918018
                httpRes.Status = (int)res.StatusCode;
                httpRes.Message = "请求失败";

                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var str = await res.Content.ReadAsStringAsync();
                    httpRes.Data = str;
                    httpRes.Message = "访问成功";
                    httpRes.Ok = true;
                }
            }
            catch (Exception ex)
            {
                httpRes.Message = $"请求异常：{ex.Message}";
            }

            return httpRes;
        }


        /// <summary>
        /// 获取商品详细
        /// </summary>
        /// <param name="user"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<HttpRes> GetItemsDetail(User user, string[] ids)
        {
            var g = new G
            {
                PageId = "VIE_SC004",
                SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate,
                Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody { G = g };
            var cc = new Dictionary<string, object>();

            //item list
            var itemList = new ArrayList();
            foreach (var id in ids)
            {
                itemList.Add(new JObject { { "item_id", id } });
            }

            var list = new List<CItem>
            {
                new CItem
                {
                    Method = Methods.GetItemDetail,
                    Option = new Dictionary<string, object>()
                    {
                        {"all_status_flag", null},
                        {"item_list", itemList},
                    }
                },
            };
            cc.Add("Item", list);
            body.C = cc;
            return await ApiRequest(user, body);
        }

        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<HttpRes> GetImage(string url)
        {
            var httpRes = new HttpRes { Ok = false, Data = "", Url = url };

            try
            {
                var client = ClientFactory();
                client.Timeout = TimeSpan.FromSeconds(60);
                var res = await client.GetAsync(url);
                httpRes.Status = (int)res.StatusCode;
                httpRes.Message = "请求失败";
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    httpRes.Ok = true;
                    httpRes.Data = await res.Content.ReadAsByteArrayAsync();
                }
            }
            catch (Exception ex)
            {
                httpRes.Message = $"请求异常：{ex.Message}";
            }

            return httpRes;
        }


        /// <summary>
        /// 切换代理
        /// </summary>
        /// <returns></returns>
        public async Task ChangeProxy()
        {
            var res = await _proxy.GetProxies();
            if (res.Ok)
            {
                if (res.Data is List<HttpProxy> data && data.Count > 0)
                {
                    config = data[0];
                    var msg = $"切换Proxy成功:{config.Ip} / {config.Port} / {config.ExpireTime}";
                    Debug.WriteLine(msg);
                    Context.LogText = msg;
                }
            }
        }

        /// <summary>
        /// 创建HttpCLient 
        /// </summary>
        /// <returns></returns>
        private HttpClient ClientFactory()
        {
            HttpClient client;
            if (config != null)
            {
                var proxy = new WebProxy(new Uri($"http://{config.Ip}:{config.Port}"));
                if (!config.Username.Equals(""))
                {
                    proxy.Credentials = new NetworkCredential(config.Username, config.Password);
                }

                var proxiedHttpClientHandler = new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = proxy
                };
                client = new HttpClient(proxiedHttpClientHandler)
                {
                    Timeout = TimeSpan.FromSeconds(60)
                };
                Debug.WriteLine($"正在使用代理访问：{config.Ip}:{config.Port}");
            }
            else
            {
                client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(60)
                };
            }

            return client;
        }

        /// <summary>
        /// 添加队列
        /// </summary>
        /// <param name="user"></param>
        /// <param name="body"></param>
        private void pushToQueue(User user, RequestBody body)
        {
            if (body.G.PageId == null) return;
            var eve = new ReqEvents { Page = body.G.PageId, Type = "view" };
            _events.Add(eve);
            if (_events.Count >= 5)
            {
                SetTrackingLog(user);
            }
        }

        /// <summary>
        /// 包装API请求
        /// </summary>
        /// <param name="user"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<HttpRes> ApiRequest(User user, RequestBody body)
        {
            var client = ClientFactory();
            //放入队列
            pushToQueue(user, body);
            var req = parseRequestMessage(user, body);
            var res = await Request(client, req);

            //其他异常了，这里进行一下重试
            if ((res.Code == 6 || res.Status > 500) && EnableProxy)
            {
                //需要重新解析
                await ChangeProxy();
                req = parseRequestMessage(user, body);
                client = ClientFactory();
                res = await Request(client, req);
            }

            //判断
            if (!res.Ok) return res;
            if (!(res.Data is ResData data)) return res;
            //判断是否IP被禁用了，如果IP被禁用那需要进行代理切换
            //授权失败
            if (data.StatusCode == 4)
            {
                //重新登录
                await AutoLogin(user);
            }

            return res;
        }

        /// <summary>
        /// parseRequestMessage
        /// </summary>
        /// <param name="user"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private HttpRequestMessage parseRequestMessage(User user, RequestBody body)
        {
            var pair = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("g", JsonConvert.SerializeObject(body.G)),
                new KeyValuePair<string, string>("c", JsonConvert.SerializeObject(body.C))
            };
            var content = new FormUrlEncodedContent(pair);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
            var req = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.shoppies.jp/api/app/v01"),
                Content = content,
                Headers =
                {
                    {
                        "User-Agent",
                        "Dalvik/2.1.0 (Linux; U; Android 5.1.1; xiaomi 6 Build/LYZ28N) ShoppiesForAndroidApp Version/3.0.15"
                    }
                }
            };
            return req;
        }

        /// <summary>
        /// 执行Request
        /// </summary>
        /// <param name="client"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<HttpRes> Request(HttpClient client, HttpRequestMessage req)
        {
            var httpRes = new HttpRes
            {
                Data = null,
                Ok = false,
                Message = "REQUEST FAIL",
                Status = 599,
                Method = req.Method.ToString()
            };
            try
            {
                client.Timeout = TimeSpan.FromSeconds(60);
                var res = await client.SendAsync(req);
                httpRes.Status = (int)res.StatusCode;
                httpRes.Message = "REQUEST SUCCESS";
                if (res.IsSuccessStatusCode)
                {
                    var html = await res.Content.ReadAsStringAsync();
                    httpRes.Str = html;
                    try
                    {
                        var data = JsonConvert.DeserializeObject<ResData>(html);
                        httpRes.Data = data;
                        httpRes.Code = data.StatusCode;
                        httpRes.Message = $"SUCCESS={data.StatusCode}";
                    }
                    catch (Exception e)
                    {
                        httpRes.Message = e.Message;
                    }

                    httpRes.Ok = res.StatusCode == HttpStatusCode.OK; //默认code

                    //判断是否是
                    if (html.IndexOf("急負荷", StringComparison.Ordinal) != -1)
                    {
                        httpRes.Code = 6;//符合
                    }
                }
            }
            catch (Exception ex)
            {
                httpRes.Ok = false;
                httpRes.Message = $"请求异常：{ex.Message}";
                Debug.WriteLine(ex.StackTrace);
            }
            finally
            {
                Debug.WriteLine(
                    $"URI：{req.RequestUri}\tRespMessage:{httpRes.Message}\tStatus={httpRes.Status}\tStr={httpRes.Str}");
            }

            return httpRes;
        }
    }
}