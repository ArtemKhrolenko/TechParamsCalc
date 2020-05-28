using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace TechParamsCalc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        Mutex mutex;

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            //Контроль одного экземпляра приложения
            bool createdNew;
            string mutName = "Приложение";


            mutex = new Mutex(true, mutName, out createdNew);
            if (!createdNew)
            {
                System.Windows.MessageBox.Show("Программа уже запущена.");// не обязательно
                this.Shutdown();
                return;
            }           
        }

        

        

       
    }
}
