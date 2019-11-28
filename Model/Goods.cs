using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Runtime.CompilerServices;
using JpGoods.Annotations;
using Newtonsoft.Json;

namespace JpGoods.Model
{
    [Table("Goods")]
    public sealed class Goods : INotifyPropertyChanged
    {
        private ulong _id;
        private int _goodsNo;
        private string _title;
        private bool _isChecked = false;
        private string _status = "";
        private string _size;
        private string _x2;
        private decimal _price;
        private string _json;
        private string _area = "";
        private string _brand = "";
        private string _brandId = "";
        private string _desc = "";
        private string _brandName = "";
        private string _shippingMethod = "";
        private string _shippingArea = "";
        private string _shippingDate = "";
        private string _shippingLiao = "";
        private string[] _images = new string[0];
        private string _imagesString = "";


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


        [Column("title")]
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(_title));
            }
        }

        [Required, StringLength(30), Column("goods_no")]
        public int GoodsNo
        {
            get => _goodsNo;
            set
            {
                _goodsNo = value;
                OnPropertyChanged(nameof(GoodsNo));
            }
        }

        [NotMapped]
        public string[] Images
        {
            get => _images;
            set
            {
                _images = value;
                OnPropertyChanged(nameof(Images));
                OnPropertyChanged(nameof(ImagesString));
            }
        }

        [Column(name: "images", TypeName = "VARCHAR"), JsonProperty("images")]
        public string ImagesString
        {
            get => _imagesString;
            set
            {
                _imagesString = value;
                OnPropertyChanged(nameof(ImagesString));
            }
        }

        [Column("shipping_liao", TypeName = "VARCHAR"), JsonProperty("shipping_liao")]
        public string ShippingLiao
        {
            get => _shippingLiao;
            set
            {
                _shippingLiao = value;
                OnPropertyChanged(nameof(ShippingLiao));
            }
        }

        [Column("shipping_method"), JsonProperty("shipping_method")]
        public string ShippingMethod
        {
            get => _shippingMethod;
            set
            {
                _shippingMethod = value;
                OnPropertyChanged(nameof(ShippingMethod));
            }
        }

        [Column("shipping_area"), JsonProperty("shipping_area")]
        public string ShippingArea
        {
            get => _shippingArea;
            set
            {
                _shippingArea = value;
                OnPropertyChanged(nameof(ShippingArea));
            }
        }

        [Column("shipping_date"), JsonProperty("shipping_date")]
        public string ShippingDate
        {
            get => _shippingDate;
            set
            {
                _shippingDate = value;
                OnPropertyChanged(nameof(ShippingDate));
            }
        }

        [Column("brand_name"), JsonProperty("brand_name")]
        public string BrandName
        {
            get => _brandName;
            set
            {
                _brandName = value;
                OnPropertyChanged(nameof(BrandName));
            }
        }


        [Column("desc"), JsonProperty("desc")]
        public string Desc
        {
            get => _desc;
            set
            {
                _desc = value;
                OnPropertyChanged(nameof(Desc));
            }
        }

        [Column("brand_id"), JsonProperty("brand_id")]
        public string BrandId
        {
            get => _brandId;
            set
            {
                _brandId = value;
                OnPropertyChanged(nameof(BrandId));
            }
        }

        [Column("brand"), JsonProperty("brand")]
        public string Brand
        {
            get => _brand;
            set
            {
                _brand = value;
                OnPropertyChanged(nameof(Brand));
            }
        }

        [Column("json"), JsonProperty("json")]
        public string Json
        {
            get => _json;
            set => _json = value;
        }


        [Column("area"), JsonProperty("area")]
        public string Area
        {
            get => _area;
            set
            {
                _area = value;
                OnPropertyChanged(nameof(Area));
            }
        }


        private string _categoryId = "";

        [Column("category_id"), JsonProperty("category_id")]
        public string CategoryId
        {
            get => _categoryId;
            set
            {
                _categoryId = value;
                OnPropertyChanged(nameof(CategoryId));
            }
        }


        private string _categoryName = "";

        [Column("category_name"), JsonProperty("category_name")]
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged(nameof(CategoryName));
            }
        }


        [Column("status")]
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        [Column("is_checked")]
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }


        [Column("size")]
        public string Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        [Column("x2")]
        public string X2
        {
            get => _x2;
            set
            {
                _x2 = value;
                OnPropertyChanged(nameof(X2));
            }
        }

        [Column("price")]
        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}