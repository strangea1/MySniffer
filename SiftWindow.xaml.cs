using demonviglu.config;
using Microsoft.Win32;
using Sniffer;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WPF_STANDARD
{
    /// <summary>
    /// SiftWindow.xaml 的交互逻辑
    /// </summary>

    public enum SiftType
    {
        TimeLimit,
        AddressLimit,
        AgreementLimit,
        ContentSearch
    }

    public class SiftData
    {
        public string Content { get; set; }
        public SiftType SiftDataType { get; set; }

        public bool IsDirty = false;

        public SiftData()
        {
            Content = string.Empty;
            SiftDataType = SiftType.TimeLimit;
        }

        public bool MEquals(SiftData other)
        {
            return Content == other.Content && SiftDataType == other.SiftDataType;
        }
    }

    public class SiftDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (SiftType)value switch
            {
                SiftType.TimeLimit => "时间筛选",
                SiftType.ContentSearch => "内容筛选",
                SiftType.AddressLimit => "地址筛选",
                SiftType.AgreementLimit => "协议筛选",
                _ => "",
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "DEFAULT";
        }
    }

    public partial class SiftWindow : Window
    {
        public ObservableCollection<SiftData> SiftDatas { get; set; }

        public event EventHandler SiftDatasChanged;

       private void OnSiftDatasChanged()
        {
            SiftDatasChanged?.Invoke(this, EventArgs.Empty);
        }


        private const string DEFAULT_TITLE = "Sift Window";

        public SiftWindow()
        {
            InitializeComponent();

            Start();
        }

        public void Start()
        {
            //Binding
            SiftDatas=new ObservableCollection<SiftData>();
            SiftDatasListView.ItemsSource = SiftDatas;

            Title = DEFAULT_TITLE;

            //Init Settings Menu
            foreach (SiftType type in Enum.GetValues(typeof(SiftType)))
            {
                MenuItem item = new()
                {
                    Header = type.ToString()
                };
                switch (type)
                {
                    case SiftType.TimeLimit:
                        item.Click += OnAddTimeSiftButtonDown;
                        break;
                    case SiftType.ContentSearch:
                        item.Click += OnAddContentSiftButtonDown;
                        break;
                    case SiftType.AddressLimit:
                        item.Click += OnAddAddressLimitSiftButtonDown;
                        break;
                    case SiftType.AgreementLimit:
                        item.Click += OnAddAgreementSiftButtonDown;
                        break;
                }
                Settings_Add_MenuItem.Items.Add(item);
            }
        }



        #region ViewInteract

        #region Menu -> Settings

        #region Menu -> Settings -> Add
        private void OnAddTimeSiftButtonDown(object sender, RoutedEventArgs e)
        {
            SiftData data = TimeSiftEditWindow.Work(new());

            if (data.IsDirty)
            {
                SiftDatas.Add(data);
                OnSiftDatasChanged();
            }
        }

        private void OnAddContentSiftButtonDown(object sender, RoutedEventArgs e)
        {
            SiftData data=ContentSearchWindow.Work(new());
            if (data.IsDirty)
            {
                SiftDatas.Add(data);
                OnSiftDatasChanged();
            }
        }

        private void OnAddAgreementSiftButtonDown(object sender, RoutedEventArgs e)
        {
            SiftData data = AgreementSiftEditWindow.Work(new());

            if(data.IsDirty)
            {
                SiftDatas.Add(data);
                OnSiftDatasChanged();
            }
        }

        private void OnAddAddressLimitSiftButtonDown(object sender, RoutedEventArgs e)
        {
            SiftData data = AddressSiftEditWindow.Work(new());

            if (data.IsDirty)
            {
                SiftDatas.Add(data);
                OnSiftDatasChanged();
            }
        }

        #endregion

        #endregion

        #endregion

        #region SiftDataListview

        #region SiftDataListview -> Popmenu

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            int index = SiftDatasListView.SelectedIndex;

            SiftData siftData = SiftDatas[index];
            switch(siftData.SiftDataType)
            {
                case SiftType.TimeLimit: 
                    siftData = TimeSiftEditWindow.Work((TimeLimitSiftData)siftData);
                    break;
                case SiftType.AddressLimit:
                    siftData = AddressSiftEditWindow.Work((AddressSiftData)siftData);
                    break;
                case SiftType.ContentSearch:
                    siftData = ContentSearchWindow.Work((ContentSearchData)siftData);
                    break;
                case SiftType.AgreementLimit:
                    siftData = AgreementSiftEditWindow.Work((AgreementSiftData)siftData);
                    break;
            }
            if (!SiftDatas[index].MEquals(siftData))
            {
                SiftDatas[index] = siftData;
                OnSiftDatasChanged();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            int index = SiftDatasListView.SelectedIndex;

            SiftDatas.RemoveAt(index);
            OnSiftDatasChanged();
        }
        #endregion

        private void SiftDatasListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int index = SiftDatasListView.SelectedIndex;

            if (index == -1) return;

            SiftData siftData = SiftDatas[index];


            switch (siftData.SiftDataType)
            {
                case SiftType.TimeLimit:
                    siftData = TimeSiftEditWindow.Work((TimeLimitSiftData)siftData);

                    if (!SiftDatas[index].MEquals(siftData))
                    {
                        SiftDatas[index] = (SiftData)siftData;
                        OnSiftDatasChanged();
                    }
                    break;
                case SiftType.AddressLimit:
                    break;
                case SiftType.AgreementLimit:
                    break;
                case SiftType.ContentSearch:
                    break;
            }
        }
        #endregion
    }
}
