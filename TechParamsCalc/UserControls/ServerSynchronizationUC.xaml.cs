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

namespace TechParamsCalc.UserControls
{
    /// <summary>
    /// Interaction logic for ServerSynchronizationUC.xaml
    /// </summary>
    
    public partial class ServerSynchronizationUC : UserControl
    {
        public string[] singleTagNamesForRW; //Имена тегов, которые нужно читать отдельно от общих групп тегов
        private string[] otherTagsFromOPC;
        public ServerSynchronizationUC()
        {
            InitializeComponent();
            
            //Восстанавлиеваем настройи из App.config
            this.ServerSyncWritingTagTextBox.Text = Properties.Settings.Default.ServerSyncWriteTag;
            this.AtmoPressureTagTextBox.Text = Properties.Settings.Default.AtmoPressureTag;
            this.OtherTagsNamesTextBlock.Text = Properties.Settings.Default.OtherTagsFromOPC.Replace(",", Environment.NewLine); 
            SaveButton.IsEnabled = false;
            
            //Создаем массив тегов, которые нужно дополнительно читать из OPC
            otherTagsFromOPC = this.OtherTagsNamesTextBlock.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            //Соединяем два массива. В будущем необходимо слелать один набор параметров!!!
            singleTagNamesForRW = new string[] { this.ServerSyncWritingTagTextBox.Text, this.AtmoPressureTagTextBox.Text }.Union(otherTagsFromOPC).ToArray();
            
        }
     

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //Сохраняем настройки в секции Settings приложения
            Properties.Settings.Default.ServerSyncWriteTag = this.ServerSyncWritingTagTextBox.Text;
            Properties.Settings.Default.AtmoPressureTag = this.AtmoPressureTagTextBox.Text;
            
            Properties.Settings.Default.Save();
            
            MessageBox.Show("For settings applying please restart application!", "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            SaveButton.IsEnabled = false;
        }

        private void AtmoPressureTagTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;
        }

        private void ServerSyncWritingTagTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;
        }
    }
}
