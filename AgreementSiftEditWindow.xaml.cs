using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_STANDARD
{
    /// <summary>
    /// AddressSiftWindow.xaml 的交互逻辑
    /// </summary>
    /// 

    public class AgreementSiftData : SiftData 
    {
        public bool TCP { get; set; }
        public bool UDP { get; set; }
        public bool IP { get; set; }
        public bool IPv6 { get; set; }
        public bool ARP { get; set; }
        public bool ICMP { get; set; }
        public bool Other { get; set; }

        public AgreementSiftData() 
        {
            TCP = false;   
            UDP = false;
            IP = false;
            IPv6 = false;
            ARP = false;
            ICMP = false;
            Other = false;
        }
    }
    public partial class AgreementSiftEditWindow : Window
    {
        private readonly AgreementSiftData Data = new();
        public AgreementSiftEditWindow()
        {
            InitializeComponent();

        }

        public static SiftData Work(AgreementSiftData data)
        {
            AgreementSiftEditWindow Instance = new();

            Instance.Data.SiftDataType = SiftType.AgreementLimit;

            Instance.TCPCheck.IsChecked = data.TCP;
            Instance.ARPCheck.IsChecked = data.ARP;
            Instance.IPCheck.IsChecked = data.IP;
            Instance.IPv6Check.IsChecked = data.IPv6;
            Instance.UDPCheck.IsChecked = data.UDP;
            Instance.ICMPCheck.IsChecked = data.ICMP;
            Instance.OtherCheck.IsChecked = data.Other;
            
            Instance.ShowDialog();

            return Instance.Data;
        }

        #region View Interact
        private void OnInputSiftContent(object sender, TextChangedEventArgs e)
        {

        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            Data.TCP = TCPCheck.IsChecked == true;
            Data.UDP = UDPCheck.IsChecked == true;
            Data.ARP = ARPCheck.IsChecked == true;
            Data.IP = IPCheck.IsChecked == true;
            Data.IPv6 = IPv6Check.IsChecked == true;
            Data.ICMP = ICMPCheck.IsChecked == true;
            Data.Other = OtherCheck.IsChecked == true;
            string tmp = "允许的协议类型：";
            if (Data.TCP) tmp += " TCP";
            if (Data.UDP) tmp += " UDP";
            if (Data.IP) tmp += " IP";
            if (Data.IPv6) tmp += " IPv6";
            if (Data.ICMP) tmp += " ICMP";
            if (Data.ARP) tmp += " ARP";
            if (Data.Other) tmp += " Other";
            Data.Content = tmp;
            Data.IsDirty = true;
            
            this.Close();
        }

        #endregion
    }
}
