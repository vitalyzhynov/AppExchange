using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ServiceModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppExchangeServer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ServiceHost service;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddClientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            addWindow.Owner = this;
            addWindow.Show();
        }

        private void RemoveClientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteWindow deleteWindow = new DeleteWindow();
            deleteWindow.Owner = this;
            deleteWindow.Show();
        }

        private void UpdateClientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindow updateWindow = new UpdateWindow();
            updateWindow.Owner = this;
            updateWindow.Show();
        }

        public void WriteToLog(string message)
        {
            LogTextBlock.Text += message;
        }

        private void StartMenuItem_Click(object sender, RoutedEventArgs e)
        {
            service = new ServiceHost(typeof(Service));
            service.Open();
            LogTextBlock.Text += DateTime.Now + " Server started!" + Environment.NewLine;
            StartMenuItem.IsEnabled = false;
            StopMenuItem.IsEnabled = true;
        }

        private void StopMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (service == null) return;

            service.Close();
            LogTextBlock.Text += DateTime.Now + " Server stopped!" + Environment.NewLine;
            StartMenuItem.IsEnabled = true;
            StopMenuItem.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (service == null) return;

            service.Close();
            LogTextBlock.Text += DateTime.Now + " Server stopped!" + Environment.NewLine;
        }
    }
}
