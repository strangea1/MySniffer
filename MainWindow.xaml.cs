using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using WPF_STANDARD;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Sniffer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class SniffData
    {
        public string Sum { get; set; } = string.Empty;
        public string Information { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string InformationAscii { get; set; } = string.Empty;
        public bool HasNext;

    }


    public partial class MainWindow : Window
    {

        public MainWindow()
        {

            InitializeComponent();

            M_Init();
        }

        private PacketSniffer PacketSniffer;

        private string SelectNetCard = string.Empty;

        ObservableCollection<SniffData> SniffDatas = new();

        ObservableCollection<string> comboxSArray = new();

        private void M_Init()
        {
            PacketSniffer = new PacketSniffer();

            PacketSniffer.OnGetAnsDeleGate = Refresh;

            BagInformationListBox.ItemsSource = SniffDatas;

            foreach (string card in PacketSniffer.GetAvailableInterfaces())
            {
                ListBoxItem item = new();
                item.Content = card;
                item.Selected += OnNetCardSeleted;
                SniffItemSourceListView.Items.Add(item);
            }
        }

        private SniffData _AnsBuffer;
        private SniffData AnsBuffer
        {
            get
            {
                return _AnsBuffer;
            }
            set
            {
                _AnsBuffer = value;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 在主线程中更新 UI 控件
                    //SniffAnsTextBox.Text += "\n=========\n"+_AnsBuffer;
                    //ListBoxItem item = new();
                    //item.Content = _AnsBuffer;
                    //item.
                    //item.Selected += OnBagSeleted;

                    SniffDatas.Add(_AnsBuffer);

                });
            }
        }

        private void Refresh(SniffData ans)
        {
            AnsBuffer = ans;
        }

        private void OnNetCardSeleted(object sender, RoutedEventArgs e)
        {
            SelectNetCard = ((ListBoxItem)sender).Content.ToString() ?? "";
            //MessageBox.Show(SelectNetCard);
        }

        private void BeginSniff_Click(object sender, RoutedEventArgs e)
        {
            BagInformationListBox.ItemsSource = SniffDatas;
            PacketSniffer.StartSniffing(SelectNetCard);
        }

        private void EndSniff_Click(object sender, RoutedEventArgs e)
        {
            PacketSniffer.StopSniffing();
        }

        private void BagInformationListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BagInformationListBox.SelectedItems.Count != 0)
            {
                SniffData data = (SniffData)(BagInformationListBox.SelectedItems[0] ?? new());
                FullInformation win = new(data);
                win.Show();
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            SiftWindow window = new();
            window.SiftDatasChanged += Window_SiftDatasChanged;
            window.Show();
            BagInformationListBox.ItemsSource = Select(window);
        }

        private void Window_SiftDatasChanged(object? sender, EventArgs e)
        {
            SiftWindow window = sender as SiftWindow;
            if (window != null)
            {
                BagInformationListBox.ItemsSource = Select(window);
            }
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            SniffDatas.Clear();
            BagInformationListBox.ItemsSource = SniffDatas;
        }

        public ObservableCollection<SniffData> Select(SiftWindow window, string timefrom = "", string timeto = "")
        {
            // 获取筛选条件
            var Tmp = window.SiftDatas;
            bool TimeLimitSet = false, AgreementSet = false;
            List<string> from = new List<string>();
            List<string> to = new List<string>();
            List<string> Agreement = new List<string>();
            List<string> Content = new List<string>();
            bool isMatch=true;
            foreach (var siftData in Tmp)
            {
                if (siftData is TimeLimitSiftData timeLimitSiftData)
                {
                    if (!TimeLimitSet)
                    {
                        timefrom = timeLimitSiftData.FromTime;
                        timeto = timeLimitSiftData.ToTime;
                        TimeLimitSet = true;
                        if (timefrom != "" && !DateTime.TryParse(timefrom, out DateTime parsedFromTime))
                        {
                            MessageBox.Show($"开始时间格式错误: {timefrom}. 请使用符合格式的时间.");
                            timefrom = "";
                            TimeLimitSet = false;
                        }
                        if (timeto != "" && !DateTime.TryParse(timefrom, out DateTime parsedToTime))
                        {
                            MessageBox.Show($"结束时间格式错误: {timeto}. 请使用符合格式的时间.");
                            timeto = "";
                            TimeLimitSet = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("有多个时间限制");
                    }

                }
                if (siftData is AddressSiftData addressSiftData)
                {
                    if(addressSiftData.From!="")
                        from.Add(addressSiftData.From);
                    if(addressSiftData.To!="")
                        to.Add(addressSiftData.To);
                }
                if (siftData is AgreementSiftData agreementSiftData)
                {
                    if (!AgreementSet)
                    {
                        if (agreementSiftData.TCP) Agreement.Add("TCP");
                        if (agreementSiftData.UDP) Agreement.Add("UDP");
                        if (agreementSiftData.IP) Agreement.Add("IP (IPv4)");
                        if (agreementSiftData.IPv6) Agreement.Add("IP (IPv6)");
                        if (agreementSiftData.ICMP) Agreement.Add("ICMP");
                        if (agreementSiftData.Other) Agreement.Add("Unknown");
                        if (agreementSiftData.ARP) Agreement.Add("ARP");
                        AgreementSet = true;
                    }
                    else
                    {
                        MessageBox.Show("重复的协议类型设置");
                    }
                }
                if (siftData is ContentSearchData contentSearchData)
                {
                    Content.Add(contentSearchData.SearchContent);
                }
            }

            // 创建 ObservableCollection 用于绑定到界面
            ObservableCollection<SniffData> filteredSniffData = new ObservableCollection<SniffData>();

            // 遍历 SniffDatasBuffer 进行筛选
            foreach (SniffData data in SniffDatas)
            {
                isMatch = true;
                // 根据 Tmp 条件进行筛选
                // 筛选：From
                if (from.Count != 0 && !from.Contains(data.From))
                {
                    continue;
                }

                // 筛选：To
                if (to.Count != 0 && !to.Contains(data.To))
                {
                    continue;
                }

                // 筛选：TimeFrom
                if (!string.IsNullOrEmpty(timefrom) && DateTime.Parse(data.Time) < DateTime.Parse(timefrom))
                {
                    continue;
                }

                // 筛选：TimeTo
                if (!string.IsNullOrEmpty(timeto) && DateTime.Parse(data.Time) > DateTime.Parse(timeto))
                {
                    continue;
                }

                // 筛选：AgreementType
                if (Agreement.Count != 0 && !Agreement.Contains(data.Type))
                {
                    continue;
                }

                // 内容搜索功能
                if (Content.Count != 0)
                {

                    // 假设 data.Information 是包含要搜索内容的字符串,必须包括搜索的全部内容
                    foreach (string SearchString in Content)
                    {
                        if (string.IsNullOrEmpty(data.Information) || !data.Information.Contains(SearchString))
                        {
                            isMatch = false;
                            continue;
                        }
                    }
                    if(!isMatch)continue;

                }
                // 如果匹配所有条件，添加到结果集
                filteredSniffData.Add(data);
            }

            // 返回筛选后的数据
            return filteredSniffData;
        }


    }
}