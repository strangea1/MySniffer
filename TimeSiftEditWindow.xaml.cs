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

    public class TimeLimitSiftData : SiftData 
    {
        public string FromTime {  get; set; }
        public string ToTime { get; set; }

        public TimeLimitSiftData() 
        {
            FromTime = string.Empty;
            ToTime = string.Empty;
        }
    }
    public partial class TimeSiftEditWindow : Window
    {
        private readonly TimeLimitSiftData Data = new();
        public TimeSiftEditWindow()
        {
            InitializeComponent();
        }

        public static SiftData Work(TimeLimitSiftData data)
        {
            TimeSiftEditWindow Instance = new();

            Instance.Data.SiftDataType = SiftType.TimeLimit;

            Instance.FromTimeBox.Text = data.FromTime;
            Instance.ToTimeBox.Text = data.ToTime;

            Instance.ShowDialog();
            return Instance.Data;
        }

        #region View Interact
        private void OnInputSiftContent(object sender, TextChangedEventArgs e)
        {

        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            Data.FromTime = FromTimeBox.Text;
            Data.ToTime = ToTimeBox.Text;
            Data.Content="开始时间："+Data.FromTime+"终止时间："+Data.ToTime;

            Data.IsDirty = true;
            
            this.Close();
        }

        private void FirstSiftContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers != ModifierKeys.Control))
            {
                ToTimeBox.Focus();
                e.Handled = true;
            }
            else 
            {
                e.Handled = false;
            }
        }

        private void SecondSiftContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers != ModifierKeys.Control))
            {
                Data.FromTime = FromTimeBox.Text;
                Data.ToTime = ToTimeBox.Text;
                Data.Content = "开始时间：" + Data.FromTime + "终止时间：" + Data.ToTime;
                Data.IsDirty = true;

                Close();
            }
            else
            {
                e.Handled = false;
            }
        }
        #endregion
    }
}
