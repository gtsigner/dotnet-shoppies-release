using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using JpGoods.Annotations;

namespace JpGoods.Windows.Import
{
    public class ImportContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _url = "https://shoppies.jp/user-shop/19745752";

        private bool _isParsing = false;
        private int _goodsCount = 0;
        private bool _isImporting = false;

        public bool IsImporting
        {
            get => _isImporting;
            set
            {
                _isImporting = value;
                OnPropertyChanged(nameof(IsImporting));
                OnPropertyChanged(nameof(CanImport));
            }
        }

        public int GoodsCount
        {
            get => _goodsCount;
            set
            {
                _goodsCount = value;
                OnPropertyChanged(nameof(GoodsCount));
            }
        }

        public bool IsParsing
        {
            get => _isParsing;
            set
            {
                _isParsing = value;
                OnPropertyChanged(nameof(IsParsing));
                OnPropertyChanged(nameof(CanParse));
            }
        }

        public bool CanParse => !_isParsing;
        public bool CanImport => !_isImporting;

        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                OnPropertyChanged(nameof(Url));
            }
        }


        #region 日志

        private readonly StringBuilder _logStringBuilder = new StringBuilder();


        /// <summary>
        /// 获取或者设置日志
        /// </summary>
        public string LogText
        {
            get => _logStringBuilder.ToString();
            set
            {
                if (value == string.Empty || _logStringBuilder.Length > 100 * 10000)
                {
                    _logStringBuilder.Clear();
                }

                var dateStr = DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture);
                _logStringBuilder.Append($"{dateStr} - {value}\n");
                OnPropertyChanged(nameof(LogText));
            }
        }

        #endregion
    }
}