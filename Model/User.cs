using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JpGoods.Annotations;
using JpGoods.Libs;
using Newtonsoft.Json;

namespace JpGoods.Model
{
    [Table("Users")]
    public class User : INotifyPropertyChanged
    {
//        private string _masterUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        private string _masterUpdate = "2019-11-26 12:07:20";
        private string _username = "";
        private string _password = "";
        private string _token = "";
        private string _sessionId = "";
        private string _uuid = "";
        private int _errorCount = 0; //登录失败次数

        public int ErrorCount
        {
            get => _errorCount;
            set
            {
                _errorCount = value;
                OnPropertyChanged(nameof(ErrorCount));
            }
        }

        [Column("uuid", TypeName = "VARCHAR"), StringLength(64), JsonProperty("uuid")]
        public string Uuid
        {
            get => _uuid;
            set
            {
                _uuid = value;
                OnPropertyChanged(nameof(Uuid));
            }
        }

        private bool _isLogin = false;


        private ulong _id;

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id")]
        public ulong Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        [Required, Column("username", TypeName = "VARCHAR"), StringLength(32),]
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        [Required, Column("password", TypeName = "VARCHAR"), StringLength(32),]
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        [Column("token", TypeName = "VARCHAR"), StringLength(64), JsonProperty("token")]
        public string Token
        {
            get => _token;
            set
            {
                _token = value;
                OnPropertyChanged(nameof(Token));
            }
        }

        [Column("session_id", TypeName = "VARCHAR"), StringLength(32), JsonProperty("session_id")]
        public string SessionId
        {
            get => _sessionId;
            set
            {
                _sessionId = value;
                OnPropertyChanged(nameof(SessionId));
            }
        }


        [Column("master_update", TypeName = "VARCHAR"), StringLength(255), JsonProperty("master_update")]
        public string MasterUpdate
        {
            get => _masterUpdate;
            set
            {
                _masterUpdate = value;
                OnPropertyChanged(nameof(MasterUpdate));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsLogin
        {
            get => _isLogin;
            set
            {
                _isLogin = value;
                OnPropertyChanged(nameof(IsLogin));
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public async Task Login()
        {
        }

        public void Init()
        {
            this.Uuid = JpUtil.GetUuid(); //随机UUID
            this.Token = JpUtil.GetApplicationToken(JpUtil.APP_VERSION, this.Uuid); //Token
        }
    }
}