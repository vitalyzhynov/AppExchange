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
using IDEAChipher;
using System.Windows.Forms;

namespace ClientWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Client currentClient;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(int id)
        {
            InitializeComponent();
            currentClient = new Client(id);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Client client = new Client(1);
            //textBlock1.Text = Convert.ToString(client.SyncKeys());
            //textBlock1.Text = client.ByteArrayToString(new byte[] { 4, 1, 2, 0});

            client.Send(/*"Some mission new", */1, "Program.cs");

            //Important!
            //ushort[] arr = { 55296, 55297, 56321};
            //textBlock1.Text = /* Convert.ToString('\uD801');*/ ExtensionClass.ToLiteralString(arr);
            
            //try
            //{
            //    Client client = new Client(1);
            //    //textBlock1.Text = Convert.ToString(client.SyncKeys());
            //    //textBlock1.Text = client.ByteArrayToString(new byte[] { 4, 1, 2, 0});
            //    client.Send("Some mission", 1);
            //}
            //catch(Exception ex)
            //{
            //    textBlock1.Text = ex.Message;
            //}
        }

        private string ByteToString(byte[] arr)
        {
            string result = string.Empty;

            if (arr != null)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    result += arr[i] + "\n\r";
                }
            }

            return result;
        }

        private void SendMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Neccessary window or controls to make active
            GetButton.Visibility = Visibility.Hidden;
            SendButton.Visibility = Visibility.Visible;
        }

        private void ReceiveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Neccessary window or controls to make active
            GetButton.Visibility = Visibility.Visible;
            SendButton.Visibility = Visibility.Hidden;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select path to directory"})
            {
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    PathTextBox.Text = fbd.SelectedPath;
                }
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            //if client is authorized
            if (currentClient != null)
            {
                if (PathTextBox.Text.Equals(string.Empty))
                {
                    System.Windows.MessageBox.Show("You should to choose the directoy before sending!");
                }
                else
                {
                    try
                    {
                        int resId;
                        if (!RecipientIdTextBox.Text.Equals(""))
                        {
                            resId = Convert.ToInt32(RecipientIdTextBox.Text);
                            currentClient.Send(resId, PathTextBox.Text);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("You should to enter the recipient id before sending!");
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

        private void GetButton_Click(object sender, RoutedEventArgs e)
        {
            //if client is authorized
            if (currentClient != null)
            {
                if (PathTextBox.Text.Equals(string.Empty))
                {
                    System.Windows.MessageBox.Show("You should to choose the directoy before sending!");
                }
                else
                {
                    try
                    {
                        int resId;
                        if (!RecipientIdTextBox.Text.Equals(""))
                        {
                            //in this case RecipientIdTextBox will contain id of the sender
                            resId = Convert.ToInt32(RecipientIdTextBox.Text);
                            currentClient.Get(resId, PathTextBox.Text);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("You should to enter the sender id before getting the data!");
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

    }
}
