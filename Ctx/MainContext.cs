using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Documents;
using JpGoods.Model;
using Microsoft.EntityFrameworkCore;

namespace JpGoods.Ctx
{
    public class MainContext : INotifyPropertyChanged
    {
        private Setting _setting = new Setting();
        private ObservableCollection<string> _areas = new ObservableCollection<string>();
        private ObservableCollection<string> _shippingMethods = new ObservableCollection<string>();
        private ObservableCollection<string> _shippingType = new ObservableCollection<string>();//配送料
        private ObservableCollection<string> _statusType = new ObservableCollection<string>();//商品状态
        private ObservableCollection<string> _shippingDates = new ObservableCollection<string>();//配送日期

        /// <summary>
        /// 配送料变，
        /// </summary>
        public ObservableCollection<string> ShippingType
        {
            get => _shippingType;
            set
            {
                _shippingType = value;
                OnPropertyChanged(nameof(ShippingType));
                //变
                
                
                
            }
        }

        public ObservableCollection<string> StatusType
        {
            get => _statusType;
            set
            {
                _statusType = value;
                OnPropertyChanged(nameof(StatusType));
            }
        }

        public ObservableCollection<string> ShippingDates
        {
            get => _shippingDates;
            set
            {
                _shippingDates = value;
                OnPropertyChanged(nameof(ShippingDates));
            }
        }

        public ObservableCollection<string> Areas
        {
            get => _areas;
            set
            {
                _areas = value;
                OnPropertyChanged(nameof(Areas));
            }
        }

        public ObservableCollection<string> ShippingMethods
        {
            get => _shippingMethods;
            set
            {
                _shippingMethods = value;
                OnPropertyChanged(nameof(ShippingMethods));
            }
        }

        public Setting Setting
        {
            get => _setting;
            set
            {
                _setting = value;
                this.OnPropertyChanged(nameof(Setting));
            }
        }

        private Status _status = new Status();


        private Goods _goods = new Goods();

        public Goods Goods
        {
            get => _goods;
            set
            {
                _goods = value;
                OnPropertyChanged(nameof(Goods));
            }
        }


        public Status Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #region 日志

        private string _logStringBuilder = "";


        /// <summary>
        /// 获取或者设置日志
        /// </summary>
        public string LogText
        {
            get => _logStringBuilder;
            set
            {
                if (value == String.Empty || _logStringBuilder.Length > 100000)
                {
                    _logStringBuilder = "";
                }

                var dateStr = DateTime.Now.ToLocalTime().ToString(CultureInfo.CurrentCulture);
                _logStringBuilder = ($"{dateStr} - {value}\n") + _logStringBuilder;
                OnPropertyChanged(nameof(LogText));
            }
        }

        #endregion
    }
}