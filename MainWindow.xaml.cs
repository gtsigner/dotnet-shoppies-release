using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JpGoods.Api;
using JpGoods.Bean;
using JpGoods.Ctx;
using JpGoods.Libs;
using JpGoods.Model;
using JpGoods.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using rv_core.Utils;
using Timer = System.Timers.Timer;

namespace JpGoods
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainContext _vm = new MainContext();
        private readonly ObservableCollection<Goods> _goodsList = new ObservableCollection<Goods>();

        private readonly ObservableCollection<User> _users = new ObservableCollection<User>();

        //执行任务的队列
        private ConcurrentQueue<ReleaseTask> _queue = new ConcurrentQueue<ReleaseTask>();

        //有序列表，表示调度循序
        private readonly SortedList<long, ReleaseTask> _tasks = new SortedList<long, ReleaseTask>();

        public readonly User Anonymous = new User();

        private Timer _releaseTimer;
        private Thread _thread;
        public readonly JpApi Api;

        public MainWindow()
        {
            InitializeComponent();
            var factory = (IHttpClientFactory) App.MyServiceProvider.GetService(typeof(IHttpClientFactory));
            Api = new JpApi(factory);
            _checkAuth();

            DataContext = _vm;
            DgGoodsList.DataContext = _goodsList;
            _vm.LogText = "APP启动成功";

            App.DbCtx.Database.EnsureCreated(); //创建
            //初始化timer
            InitTimer();
            //获取
            _loadList();
            _initThread();
            _initContext();
            _initCacheBrands(); //初始化缓存
            _initAnonymous(); //初始化匿名后进行brand初始化
        }

        /// <summary>
        /// 判断软件是否付费可用
        /// </summary>
        private async void _checkAuth()
        {
            var factory = (IHttpClientFactory) App.MyServiceProvider.GetService(typeof(IHttpClientFactory));
            var client = factory.CreateClient("blog.oeynet.com");
            var req = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://web.nike.oeynet.com/api/jpgoods/status")
            };
            var res = await Api.Request(client, req);
            if (res.Ok == false)
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// 初始化匿名
        /// </summary>
        private async void _initAnonymous()
        {
            //1.创建匿名用户
            try
            {
                _vm.LogText = "初始化匿名用户访问API";
                var user = Anonymous;
                user.Init();
                //2.登录临时用户
                var tmp = await Api.RegisterUserTemp(user.Uuid);
                if (tmp.Ok)
                {
                    var data = tmp.Data as ResData;
                    var u = data?.User;
                    if (data?.StatusCode != 1)
                    {
                        throw new Exception(data?.Error.Message);
                    }

                    //sessionID
                    user.SessionId = u?.GetValue("registUserTemp")
                        .ToObject<JObject>().GetValue("session_id")
                        .ToString();
                    _vm.LogText = $"匿名用户登录成功:{user.SessionId}";
                    user.IsLogin = true; //登录成功
                    this._initJpBrands(); //初始化品牌
                }
                else
                {
                    throw new Exception(tmp.Message);
                }
            }
            catch (Exception ex)
            {
                _vm.LogText = $"匿名用户登录失败:{ex.Message},5s 后会进行重试";
                //重试
//                Thread.Sleep(TimeSpan.FromSeconds(5));//导致主线程阻塞

                await Task.Delay(TimeSpan.FromSeconds(5));
                _initAnonymous();
            }
        }

        private static readonly string BRAND_CACHE_FILE = Path.Combine("Goods", "brands.json");

        /// <summary>
        /// 初始化品牌
        /// </summary>
        private async void _initJpBrands()
        {
            try
            {
                //初始化收货地址之类的数据
                _vm.LogText = "初始化APP的品牌列表";
                var res = await Api.GetBrandList(Anonymous);
                if (!res.Ok)
                {
                    throw new Exception(res.Message);
                }

                var data = res.Data as ResData;
                if (data?.StatusCode != 1)
                {
                    throw new Exception(data?.Error.Message);
                }

                var brands = data.MastInfo.GetValue("getBrandList").ToObject<JObject>().GetValue("list")
                    .ToObject<List<Brand>>();

                JpConfig.BrandList.Clear();
                brands.ForEach((brand) =>
                {
                    JpConfig.BrandList.Add(new KeyValue {Title = brand.alpha_name, Value = brand.brand_id + ""});
                });
                _vm.LogText = $"初始化APP的品牌列表：OK / {brands.Count}个";

                //写入配置文件
                File.WriteAllText(BRAND_CACHE_FILE, JsonConvert.SerializeObject(brands));
            }
            catch (Exception ex)
            {
                _vm.LogText = $"初始化APP的品牌失败:{ex.Message}";
            }
        }

        private async void _initCacheBrands()
        {
            var cacheFile = BRAND_CACHE_FILE;
            var cacheDir = Path.Combine("Goods");
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            if (File.Exists(cacheFile))
            {
                var str = File.ReadAllText(cacheFile);
                var brands = JsonConvert.DeserializeObject<List<Brand>>(str);
                JpConfig.BrandList.Clear();
                brands.ForEach((brand) =>
                {
                    JpConfig.BrandList.Add(new KeyValue {Title = brand.alpha_name, Value = brand.brand_id + ""});
                });
                _vm.LogText = $"初始化APP的品牌列表：OK / {brands.Count}个";
            }
        }

        private void _initContext()
        {
            //转化成list
            var areas = new ObservableCollection<string>();
            foreach (var keyValue in JpConfig.Areas)
            {
                areas.Add(keyValue.Title);
            }

            _vm.Areas = areas;

            //发货方式
            var ships = new ObservableCollection<string>();
            foreach (var keyValue in JpConfig.ShippingMethods)
            {
                ships.Add(keyValue.Title);
            }

            _vm.ShippingMethods = ships;
        }

        #region 任务初始化

        private void _initThread()
        {
            _thread = new Thread(ThreadRunner) {IsBackground = true};
            _thread.Start();
        }

        private void InitTimer()
        {
            _releaseTimer = new Timer {Enabled = true, Interval = 1000};
            _releaseTimer.Elapsed += ReleaseTimerUp;
            _releaseTimer.Start();
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReleaseTimerUp(object sender, ElapsedEventArgs e)
        {
            lock (_tasks)
            {
                if (!_vm.Status.IsRunning) return; //任务不可调度
                if (_tasks.Count <= 0 || _queue.Count > 0) return; //提取一个，然后判断时间
                //调度
                try
                {
                    var time = Logger.GetTimeStampMic();
                    var task = _tasks.First((pair) => pair.Key <= time);
                    _tasks.Remove(task.Key); //删除task
                    _queue.Enqueue(task.Value); //立即执行
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("未找到合适的任务");
                }
            }
        }

        /// <summary>
        /// 线程循环自动调度
        /// </summary>
        private async void ThreadRunner()
        {
            while (true)
            {
                //如果程序停止
                if (_queue.TryDequeue(out var task))
                {
                    await DoReleaseTask(task);
                }

                Thread.Sleep(1000);
            }
        }

        #endregion

        #region 任务执行

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <returns></returns>
        private async Task<bool> DoReleaseTask(ReleaseTask task)
        {
            if (_users.Count <= 0) return false;
            _vm.LogText = $"开始执行任务：{task.Username} / 商品:{task.GoodsNo}";

            //查找user
            var user = _users.First(u => u.Username == task.Username);

            //发布商品
            var goods = _goodsList.First(g => g.GoodsNo == task.GoodsNo);

            if (user == null || goods == null)
            {
                _vm.LogText = $"任务执行失败:{task.Username}找不到对应信息";
            }

            var isLogin = await UserLogin(user); //检测是否已经登录
            if (!isLogin)
            {
                //任务失败
                _vm.LogText = $"任务:{task.Username} / 商品:{task.GoodsNo} 执行失败";
                return false;
            }

            try
            {
                var bean = JpParse.ParseGoodsToSaleBean(goods);
                bean.ItemId = 0;
                //判断是否使用图片缓存
                if (goods.ImagesString.Length <= 0)
                {
                    var images = await UploadImages(user, goods); //上传图片格式 x,x,x,x
                    if (images.Equals(""))
                    {
                        throw new Exception("图片未上传成功");
                    }

                    bean.ImgList = images;
                }
                else
                {
                    bean.ImgList = goods.ImagesString;
                }

                goods.ReleaseStatus = "正在发布";
                bean.Mode = 2;
                var ret = await Api.SetItem(user, bean);
                if (!ret.Ok)
                {
                    throw new Exception(ret.Message);
                }

                var data = ret.Data as ResData;
                if (data?.StatusCode != 1)
                {
                    throw new Exception(data?.Error.Message);
                }

                goods.ReleaseStatus = "发布成功";
                _vm.LogText = $"{task.Username} / 商品：{goods.GoodsNo} / 发布成功";
            }
            catch (Exception ex)
            {
                //
                Debug.WriteLine(ex);
                _vm.LogText = $" {task.Username} / 商品：{goods.GoodsNo} / 发布失败 / {ex.Message}";
                goods.ReleaseStatus = "发布失败";
            }

            return true;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<bool> UserLogin(User user)
        {
            try
            {
                if (user.IsLogin == false)
                {
                    //1.初始化用户
                    _vm.LogText = $"正在登录账号:{user.Username}";
                    user.Init();
                    //2.登录临时用户
                    var tmp = await Api.RegisterUserTemp(user.Uuid);
                    if (tmp.Ok)
                    {
                        var data = tmp.Data as ResData;
                        var u = data?.User;
                        if (u == null)
                        {
                            throw new Exception($"{user.Username} 登录失败 / {data?.Error.Message}");
                        }

                        //sessionID
                        user.SessionId = u?.GetValue("registUserTemp").ToObject<JObject>().GetValue("session_id")
                            .ToString();
                    }
                    else
                    {
                        throw new Exception($"{user.Username} 登录失败 / {tmp.Message}");
                    }

                    //进行登录
                    var res = await Api.Login(user.SessionId, user.Uuid, user.Username, user.Password);
                    if (res.Ok)
                    {
                        var data = res.Data as ResData;
                        if (data?.StatusCode != 1)
                        {
                            throw new Exception($"{user.Username} 登录失败 / {data?.Error.Message}");
                        }

                        if (data?.StatusCode == 1)
                        {
                            _vm.LogText = $"{user.Username} / 登录成功 / session:{user.SessionId}";
                            user.IsLogin = true;
                        }
                    }
                    else
                    {
                        throw new Exception($"{user.Username} 登录失败 / {tmp.Message}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _vm.LogText = $"登录失败: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// 发布商品
        /// </summary>
        /// <param name="user"></param>
        /// <param name="goods"></param>
        /// <returns></returns>
        private void ReleaseGoods(User user, Goods goods)
        {
            lock (_tasks)
            {
                //提取一个，然后判断时间
                if (_tasks.Count > 0)
                {
                    //调度
                }
            }
        }


        //图片数组
        private async Task<string> UploadImages(User user, Goods goods)
        {
            //获取本地的图片，然后依次上传
            var path = Path.Combine("Goods", goods.GoodsNo + "");
            if (!Directory.Exists(path)) return "";
            var files = Directory.GetFiles(path);
            var list = new List<string>();
            foreach (var file in files)
            {
                //读取文件，然后上传
                try
                {
                    var ext = Path.GetExtension(file);
                    if (ext != ".jpg" && ext != ".png")
                    {
                        continue;
                    }

                    _vm.LogText = $"正在上传图片：{file}";
                    var bytes = File.ReadAllBytes(file);
                    var filename = Logger.GetTimeStampMic() + ext; //文件名
                    await Task.Delay(TimeSpan.FromSeconds(2)); //延迟2秒上传图片
                    var res = await Api.UploadImage(user, filename, bytes);
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

                    //获取图片的ID
                    var imageId = data.Sales.GetValue("setPhoto")
                        .ToObject<JObject>().GetValue("image_id")
                        .ToString();

                    list.Add(imageId);
                    _vm.LogText = $"图片:{file} / upload success / ID={imageId}";
                }
                catch (Exception ex)
                {
                    _vm.LogText = $"上传图片失败：{ex.Message}";
                }
            }

            return string.Join(",", list);
        }

        #endregion

        #region 任务调度 事件绑定

        /// <summary>
        /// 点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            if (btn.Name.Equals(BtnChoose.Name))
            {
                var dialog = new OpenFileDialog {Filter = "文本文件 (*.txt)|*.txt", RestoreDirectory = true};
                if (dialog.ShowDialog() == true)
                {
                    _vm.LogText = $"用户选择了账号文件：{dialog.FileName}";
                    _vm.Status.AccountFile = dialog.FileName;
                    LoadUsers(_vm.Status.AccountFile);
                }
            }

            if (btn.Name.Equals(BtnSave.Name))
            {
                if (_vm.Goods != null)
                {
                    //验证字段是否
                    SaveGoods(_vm.Goods);
                }
            }

            if (btn.Name.Equals(BtnImport.Name))
            {
                var im = new ImportWindow(this);
                im.Closed += ImportWindowClosed;
                im.Show();
            }

            if (btn.Name.Equals(BtnRefresh.Name))
            {
                //查询
                _loadList();
            }

            if (btn.Name.Equals(BtnRestart.Name))
            {
                RestartTask();
            }

            if (btn.Name.Equals(BtnStartRelease.Name))
            {
                StartTask();
            }

            if (btn.Name.Equals(BtnStopRelease.Name))
            {
                StopTask();
            }
        }


        /// <summary>
        /// 保存商品
        /// </summary>
        /// <param name="goods"></param>
        private async void SaveGoods(Goods goods)
        {
            if (goods == null) return;
            //解析goods的具体配置之类的
            var cate = JpConfig.GetCateByTitle(JpConfig.Categories, goods.CategoryName);
            if (cate == null)
            {
                MessageBox.Show($"找不到分类：{goods.CategoryName}，保存失败");
                return;
            }

            goods.CategoryId = cate.Value;

            var brand = JpConfig.GetCateByTitle(JpConfig.BrandList, goods.BrandName);
            if (brand == null)
            {
                MessageBox.Show($"找不到品牌名称：{goods.BrandName}，保存失败");
                return;
            }

            goods.BrandId = brand.Value;

            App.DbCtx.Goods.Update(_vm.Goods);
            await App.DbCtx.SaveChangesAsync();
            _loadList();
        }

        private void ImportWindowClosed(object sender, EventArgs e)
        {
            _loadList();
        }

        /// <summary>
        /// 加载用户文件
        /// </summary>
        /// <param name="file"></param>
        private async void LoadUsers(string file)
        {
            var sr = File.OpenText(file);
            var str = await sr.ReadToEndAsync();
            sr.Close();

            str = str.Trim();
            var accounts = str.Split('\n');
            var line = 0;
            _users.Clear();
            foreach (var account in accounts)
            {
                line++;
                var sps = Regex.Split(account.Replace("\r", ""), "----");
                if (sps.Length != 2)
                {
                    _vm.LogText = $"第:{line}行 格式错误!!!";
                    break;
                }

                var user = new User {Username = sps[0].Trim(), Password = sps[1].Trim()};
                _users.Add(user);
            }

            _vm.LogText = $"账号文件验证成功:{_users.Count}个";
        }

        private void RestartTask()
        {
            ClearTasks();
            StartTask();
        }

        private void StartTask()
        {
            //判断文件是否存在
            if (_users.Count <= 0)
            {
                _vm.LogText = "当前无任何用户";
                return;
            }

            //1.生成任务，加入到队列
            lock (_tasks)
            {
                if (_tasks.Count <= 0)
                {
                    //添加任务，计算执行时间
                    var count = 0;
                    var execTime = Logger.GetTimeStampMic(); //当前
                    foreach (var user in _users)
                    {
                        foreach (var goods in _goodsList)
                        {
                            if (goods.IsChecked == false) continue;
                            var task = new ReleaseTask {Username = user.Username, GoodsNo = goods.GoodsNo};
                            count++;
                            lock (task)
                            {
                                execTime += (1 + _vm.Status.ReleaseInterval * 1000);
                                _tasks.Add(execTime, task); //添加任务
                            }
                        }

                        var changeTime =
                            long.Parse(TimeSpan.FromMinutes(_vm.Status.ChangeInterval).TotalMilliseconds + "");
                        execTime += changeTime;
                    }

                    _vm.LogText = $"加入:{count}个任务到有序列表";
                }
            }

            _vm.LogText = "任务已启动";
            _vm.Status.IsRunning = true;
        }

        private void StopTask()
        {
            _vm.Status.IsRunning = false;
        }


        /// <summary>
        /// 发布任务
        /// </summary>
        private void SendTasks()
        {
        }

        /// <summary>
        /// 清空任务
        /// </summary>
        private void ClearTasks()
        {
            lock (_tasks)
            {
                _vm.Status.CurrentGoodsIndex = 0;
                _vm.Status.CurrentUserIndex = 0;
                _tasks.Clear();
            }

            _vm.LogText = "任务已停止";
        }


        /// <summary>
        /// 加载列表
        /// </summary>
        private async void _loadList()
        {
            var iq = from s in App.DbCtx.Goods select s;
            var list = await iq.ToListAsync();
            _vm.LogText = $"获取到数据:{list.Count}";
            //clear
            _goodsList.Clear();
            list.ForEach(_goodsList.Add);
        }

        /// <summary>
        /// 改变保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgGoodsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.Goods = (Goods) DgGoodsList.SelectedItem;
            if (_vm.Goods != null)
            {
                ShowImages(_vm.Goods.GoodsNo);
            }
        }

        /// <summary>
        /// 展示图片
        /// </summary>
        /// <param name="goodsNo"></param>
        private void ShowImages(int goodsNo)
        {
            //清空控件
            //LvImg.Items.Clear();
            LvImg.ItemsSource = null; //清空绑定

            //图片文件夹不存在
            var path = Path.Combine("Goods", goodsNo + "");
            if (!Directory.Exists(path)) return;
            //TODO 判断文件的扩展名

            //获取图片列表
            var files = Directory.GetFiles(path);
            var imgList = new ObservableCollection<ImgShow>();
            foreach (var file in files)
            {
                //创建控件，TODO 直接读取到内存
                var ext = Path.GetExtension(file);
                if (ext != ".jpg" && ext != ".png")
                {
                    continue;
                }

                var fs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
                var bmp = new BitmapImage(new Uri(fs));
                var dt = new ImgShow {Image = bmp, Title = file};
                imgList.Add(dt);
            }

            LvImg.ItemsSource = imgList;
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckAll(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}