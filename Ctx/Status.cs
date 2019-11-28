using System.ComponentModel;
using System.Runtime.CompilerServices;
using JpGoods.Annotations;

namespace JpGoods.Ctx
{
    public class Status : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isRunning = false;
        private string _accountFile = "";
        private int _changeInterval = 120;
        private int _releaseInterval = 10;
        private int _currentUserIndex = 0;
        private int _currentGoodsIndex = 0;
        private bool _isCheckAll = false;

        public bool IsCheckAll
        {
            get => _isCheckAll;
            set
            {
                _isCheckAll = value;
                OnPropertyChanged(nameof(IsCheckAll));
            }
        }

        public int CurrentUserIndex
        {
            get => _currentUserIndex;
            set
            {
                _currentUserIndex = value;
                OnPropertyChanged(nameof(CurrentUserIndex));
            }
        }

        public int CurrentGoodsIndex
        {
            get => _currentGoodsIndex;
            set
            {
                _currentGoodsIndex = value;
                OnPropertyChanged(nameof(CurrentGoodsIndex));
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(IsStop));
            }
        }

        public bool IsStop => !_isRunning;

        public string AccountFile
        {
            get => _accountFile;
            set
            {
                _accountFile = value;
                OnPropertyChanged(nameof(AccountFile));
            }
        }

        public int ChangeInterval
        {
            get => _changeInterval;
            set
            {
                _changeInterval = value;
                OnPropertyChanged(nameof(ChangeInterval));
            }
        }

        public int ReleaseInterval
        {
            get => _releaseInterval;
            set
            {
                _releaseInterval = value;
                OnPropertyChanged(nameof(ReleaseInterval));
            }
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}