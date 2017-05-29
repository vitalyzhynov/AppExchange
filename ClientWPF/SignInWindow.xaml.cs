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

namespace ClientWPF
{
    /// <summary>
    /// Логика взаимодействия для SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        public SignInWindow()
        {
            InitializeComponent();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            int neccessaryId;

            Client clientForSignIn = new Client();
            string log = loginTextBox.Text;
            string pas = passwordTextBox.Text;
            try
            {
                neccessaryId = clientForSignIn.SignIn(log, pas);

                if (neccessaryId != -1)
                {
                    ReceiveWindow next = new ReceiveWindow(neccessaryId);
                    next.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Login or password is incorrect!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Server is unavailable!");
            }
        }
    }
}
