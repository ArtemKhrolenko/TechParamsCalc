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
    /// Interaction logic for LogUC.xaml
    /// </summary>
    public partial class LogUC : UserControl
    {
        public LogUC()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> stringsInLog = LogTextBlock.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            string tmpString = stringsInLog.LastOrDefault(s => s.Contains("Server is started"));


            LogTextBlock.Text = tmpString + Environment.NewLine;
        }
    }
}
