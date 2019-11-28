namespace JpGoods.Bean
{
    public class ReleaseTask
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 商品编号
        /// </summary>
        public int GoodsNo { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public int Type { get; set; }

        //执行完任务后休眠多少秒
        public int Sleep { get; set; }
    }
}