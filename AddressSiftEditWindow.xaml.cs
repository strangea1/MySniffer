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
    /// TimeSiftEditWindow.xaml 的交互逻辑
    /// </summary>
    /// 

    public class AddressSiftData : SiftData 
    {
        public string From {  get; set; }
        public string To { get; set; }

        public AddressSiftData() 
        {
            From = string.Empty;
            To = string.Empty;
        }
    }
    public partial class AddressSiftEditWindow : Window
    {
        private readonly AddressSiftData Data = new();
        public AddressSiftEditWindow()
        {
            InitializeComponent();
        }

        public static SiftData Work(AddressSiftData data)
        {
            AddressSiftEditWindow Instance = new();

            Instance.Data.SiftDataType = SiftType.AddressLimit;

            Instance.FromBox.Text = data.From;
            Instance.ToBox.Text = data.To;

            Instance.ShowDialog();



            return Instance.Data;
        }

        #region View Interact
        private void OnInputSiftContent(object sender, TextChangedEventArgs e)
        {

        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            Data.From = FromBox.Text;
            Data.To = ToBox.Text;
            Data.Content="来源IP："+Data.From+" 目的IP："+Data.To;

            Data.IsDirty = true;
            this.Close();
        }

        private void FirstSiftContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers != ModifierKeys.Control))
            {
                ToBox.Focus();
                e.Handled = true;             
            }
            else 
            {
                e.Handled = false;
            }
        }

        #endregion

        private void SecondSiftContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers != ModifierKeys.Control))
            {
                Data.From = FromBox.Text;
                Data.To = ToBox.Text;
                Data.Content = "来源IP：" + Data.From + " 目的IP：" + Data.To;
                Data.IsDirty = true;
                Close();
                e.Handled= true;
            }
            else { e.Handled = false; }
        }
    }
}
