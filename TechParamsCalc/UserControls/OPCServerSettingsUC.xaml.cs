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

namespace TechParamsCalc
{
    /// <summary>
    /// Interaction logic for OPCServerSettingsUC.xaml
    /// </summary>
    public partial class OPCServerSettingsUC : UserControl
    {
        public bool isEnableWritingChecked { get; set; }
        public OPCServerSettingsUC()
        {
            InitializeComponent();
            WritingEnableCheckBox.DataContext = this;

        }

        

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.OPCServerNameSettingTextBox.Text = Properties.Settings.Default.OPCServerName;
            this.OPCServerSubDescSettingTextBox.Text = Properties.Settings.Default.OPCServerSubString;
            SaveButton.IsEnabled = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.OPCServerName = this.OPCServerNameSettingTextBox.Text;
            Properties.Settings.Default.OPCServerSubString = this.OPCServerSubDescSettingTextBox.Text;

            Properties.Settings.Default.Save();
            SaveButton.IsEnabled = false;
        }

        private void OPCServerNameSettingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;
        }
       

        private void OPCServerSubDescSettingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;
        }
    }
}
