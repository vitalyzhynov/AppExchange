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
using System.Windows.Shapes;
using ClientModel;

namespace ClientWPF
{
    /// <summary>
    /// Логика взаимодействия для ReceiveWindow.xaml
    /// </summary>
    public partial class ReceiveWindow : Window
    {

        Client currentClient;
        int intSender = -1;
        List<IdLoginClient> allClients;
        IdLoginClient recipientInstance;

        public ReceiveWindow()
        {
            InitializeComponent();
        }

        public ReceiveWindow(int id)
        {
            InitializeComponent();
            currentClient = new Client(id);
        }

        private void grid_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> result = null;// = new List<string>();

            if (currentClient != null)
            {
                result = currentClient.GetSenders();
            }
            grid.ItemsSource = result.Select(s => new StringContainer() { Value = s }).ToList();
        }

        private void grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string senderId = (grid.SelectedItem as StringContainer).ToString();
            intSender = Convert.ToInt32(senderId);
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            //if client is authorized
            if (currentClient != null)
            {
                if (DirectoryTextBox.Text.Equals(string.Empty))
                {
                    System.Windows.MessageBox.Show("You should to choose the directoy before sending!");
                }
                else
                {
                    try
                    {
                        if (intSender != -1)
                        {
                            currentClient.Get(intSender, DirectoryTextBox.Text);
                            MessageBox.Show("Directory downloaded!");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("You should to enter the sender id on the table!");
                        }

                    }
                    catch (FormatException fex)
                    {
                        System.Windows.MessageBox.Show("Id has invalid format!" + fex.Message);
                    }
                    catch (OverflowException oex)
                    {
                        System.Windows.MessageBox.Show(
                            string.Format("Entered id is to small or to large! The borders is [{0};{1}]" + oex.Message, Int32.MinValue, Int32.MaxValue));
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Something wrong!" + ex.Message);
                    }
                }
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog() { Description = "Select path to directory" })
            {
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DirectoryTextBox.Text = fbd.SelectedPath;
                }
            }
        }

        private void choosingRecipientGrid_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (currentClient != null)
            {
                if (allClients == null)
                {
                    allClients = currentClient.GetAllClients();
                }
                //result = new List<IdLoginClient>();
                //result.Add(new IdLoginClient() { Id = 2, Login = "kjsfhkshdf" });
            }

            choosingRecipientGrid.ItemsSource = allClients.Select(s => new IdLoginClient() { Id = s.Id, Login = s.Login }).ToList();
        }

        private void choosingRecipientGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            recipientInstance = choosingRecipientGrid.SelectedItem as IdLoginClient;
        }

        class StringContainer
        {
            public string Value { get; set; }

            public override string ToString()
            {
                return Value;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

            //if client is authorized
            if (currentClient != null)
            {
                if (recipientInstance == null)
                {
                    System.Windows.MessageBox.Show("You should to choose the recipient in the table before sending!");
                }
                else if (DirectoryFromTextBox.Text.Equals(string.Empty))
                {
                    System.Windows.MessageBox.Show("You should to choose the directoy before sending!");
                }
                else
                {
                    try
                    {
                        int resId;
                        if (!recipientInstance.Id.Equals(""))
                        {
                            resId = Convert.ToInt32(recipientInstance.Id);
                            currentClient.Send(resId, DirectoryFromTextBox.Text);
                            MessageBox.Show("Sending succeed!");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("This recipient has no Id!!!");
                        }

                    }
                    catch (FormatException fex)
                    {
                        System.Windows.MessageBox.Show("Id has invalid format!" + fex.Message);
                    }
                    catch (OverflowException oex)
                    {
                        System.Windows.MessageBox.Show(
                            string.Format("Entered id is to small or to large! The borders is [{0};{1}]" + oex.Message, Int32.MinValue, Int32.MaxValue));
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Something wrong!" + ex.Message);
                    }
                }
            }
        }

        private void BrowseFromButton_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog() { Description = "Select path to directory" })
            {
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DirectoryFromTextBox.Text = fbd.SelectedPath;
                }
            }
        }
    }
}
