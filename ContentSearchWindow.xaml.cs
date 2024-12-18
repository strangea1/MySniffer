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

    public class ContentSearchData : SiftData 
    {
        public string SearchContent {  get; set; }
        public ContentSearchData() 
        {
            SearchContent = string.Empty;
        }
    }
    public partial class ContentSearchWindow : Window
    {
        private readonly ContentSearchData Data = new();
        public ContentSearchWindow()
        {
            InitializeComponent();
        }

        public static SiftData Work(ContentSearchData data)
        {
            ContentSearchWindow Instance = new();

            Instance.Data.SiftDataType = SiftType.ContentSearch;

            Instance.ContentSearchBox.Text = data.SearchContent;

            Instance.ShowDialog();



            return Instance.Data;
        }

        #region View Interact

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            Data.SearchContent = ContentSearchBox.Text;
            Data.Content = "搜索内容："+Data.SearchContent;
            Data.IsDirty = true;
            
            this.Close();
        }

        private void SiftContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers != ModifierKeys.Control))
            {
                Data.SearchContent = ContentSearchBox.Text;
                Data.Content = "搜索内容：" + Data.SearchContent;
                Data.IsDirty = true;
                e.Handled = true;
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
