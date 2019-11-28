using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JpGoods.Bean;
using JpGoods.Libs;
using JpGoods.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using rv_core.Inters;
using rvcore.Model;

namespace JpGoods.Api
{
    public class JpApi : IHttpRequest
    {
        private readonly IHttpClientFactory _factory;

        public JpApi(IHttpClientFactory factory)
        {
            _factory = factory;
        }


        public async Task<HttpRes> Login(string sessionId, string uuid, string username, string password)
        {
            var token = JpUtil.GetApplicationToken(JpUtil.APP_VERSION, uuid);
            var g = new G {PageId = "VIE_SC004", SessionId = sessionId, Token = token, Version = JpUtil.APP_VERSION};
            var body = new RequestBody {G = g};
            var cc = new Dictionary<string, object>();
            var c = new CItem
            {
                Method = Methods.Login,
                Option = new JObject {{"mail_address", username}, {"password", password}, {"uuid", uuid}}
            };
            var list = new List<CItem> {c};

            cc.Add("Auth", list);
            body.C = cc;
            return await ApiRequest(body);
        }

        /// <summary>
        /// 自动登录
        /// </summary>
        /// <returns></returns>
        public async Task<HttpRes> AutoLogin(string uuid)
        {
            var token = JpUtil.GetApplicationToken(JpUtil.APP_VERSION, uuid);
            var g = new G {PageId = "SLI_MR001", SessionId = null, Token = token, Version = JpUtil.APP_VERSION};
            var body = new RequestBody {G = g};
            var cc = new Dictionary<string, object>();
            var c = new CItem
            {
                Method = Methods.AutoLogin,
                Option = new JObject {{"uuid", uuid}}
            };
            var list = new List<CItem> {c};
            cc.Add("Auth", list);
            body.C = cc;
            return await ApiRequest(body);
        }

        /// <summary>
        /// 注册临时用户返回sessionId
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns>Task</returns>
        public async Task<HttpRes> RegisterUserTemp(string uuid)
        {
            var token = JpUtil.GetApplicationToken(JpUtil.APP_VERSION, uuid);
            var g = new G {PageId = "SLI_MR001", SessionId = null, Token = token, Version = JpUtil.APP_VERSION};
            var body = new RequestBody {G = g};
            var cc = new Dictionary<string, object>();
            var c = new CItem
            {
                Method = Methods.RegistUserTemp,
                Option = new JObject {{"uuid", uuid}}
            };
            var list = new List<CItem> {c};
            cc.Add("User", list);
            body.C = cc;
            var res = await ApiRequest(body);
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
                PageId = "MYM_SH001", SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate, Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody {G = g};
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
            return await ApiRequest(body);
        }

        /// <summary>
        /// 获取品牌列表
        /// </summary>
        /// <returns></returns>
        public async Task<HttpRes> GetBrandList(User user)
        {
            var g = new G
            {
                PageId = "SEL_EX017", SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate, Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody {G = g};
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
            return await ApiRequest(body);
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
            var token = JpUtil.GetApplicationToken(JpUtil.APP_VERSION, user.Uuid);
            var g = new G
            {
//                PageId = "SEL_EX001",
                MasterUpdate = user.MasterUpdate,
                PageId = "SEL_EX010",
                Token = token,
                Version = JpUtil.APP_VERSION,
                SessionId = user.SessionId
            };

            var body = new RequestBody {G = g};
            var c = new Dictionary<string, object>();
            var cItems = new List<CItem> {new CItem {Method = "setPhoto", Option = null}};
            c.Add("Sales", cItems);

            body.C = c;
            var client = _factory.CreateClient("api.shoppies.jp");
            client.Timeout = TimeSpan.FromSeconds(60);

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
            return await Request(client, req);
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
                PageId = "SEL_EX001", SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate, Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody {G = g};
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
            return await ApiRequest(body);
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
                PageId = "SEL_EX001", SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate, Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody {G = g};
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
            return await ApiRequest(body);
        }

        public void Logout()
        {
        }

        public async Task<HttpRes> ViewUrl(string url)
        {
            var httpRes = new HttpRes {Ok = false, Data = "", Url = url};
            try
            {
                var client = _factory.CreateClient("shoppies.jp");
                client.Timeout = TimeSpan.FromSeconds(60);

                var res = await client.GetAsync(new Uri(url));
                //https://shoppies.jp/user-item/165918018
                httpRes.Status = (int) res.StatusCode;
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
                PageId = "VIE_SC004", SessionId = user.SessionId,
                MasterUpdate = user.MasterUpdate, Token = user.Token,
                Version = JpUtil.APP_VERSION
            };
            var body = new RequestBody {G = g};
            var cc = new Dictionary<string, object>();

            //item list
            var itemList = new ArrayList();
            foreach (var id in ids)
            {
                itemList.Add(new JObject {{"item_id", id}});
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
            return await ApiRequest(body);
        }

        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<HttpRes> GetImage(string url)
        {
            var httpRes = new HttpRes {Ok = false, Data = "", Url = url};

            try
            {
                var client = _factory.CreateClient("api.shoppies.jp");
                client.Timeout = TimeSpan.FromSeconds(60);
                var res = await client.GetAsync(url);
                httpRes.Status = (int) res.StatusCode;
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

        public async Task<HttpRes> ApiRequest(RequestBody body)
        {
            var client = _factory.CreateClient("api.shoppies.jp");
            client.Timeout = TimeSpan.FromSeconds(60);

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
            return await Request(client, req);
        }

        public async Task<HttpRes> Request(HttpClient client, HttpRequestMessage req)
        {
            var httpRes = new HttpRes
            {
                Data = null,
                Ok = false,
                Message = "REQUEST FAIL",
                Status = 200,
                Method = req.Method.ToString()
            };
            try
            {
                client.Timeout = TimeSpan.FromSeconds(60);
                var res = await client.SendAsync(req);
                httpRes.Status = (int) res.StatusCode;
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
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }

                    httpRes.Ok = res.StatusCode == HttpStatusCode.OK; //默认code
                }
            }
            catch (Exception ex)
            {
                httpRes.Ok = false;
                httpRes.Message = $"请求异常：{ex.Message}";
            }
            finally
            {
                Console.WriteLine($"URI：{req.RequestUri}\tRespMessage:{httpRes.Message}\tStatus={httpRes.Status}");
            }

            return httpRes;
        }
    }
}