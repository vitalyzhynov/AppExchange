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

namespace AppExchangeServer
{
    /// <summary>
    /// Логика взаимодействия для UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {

        private UsersContext usersContext;

        public UpdateWindow()
        {
            InitializeComponent();
        }

        private void UpdateClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (usersContext == null)
            {
                usersContext = new UsersContext();
            }

            try
            {
                if (idTextBox.Text == string.Empty)
                {
                    MessageBox.Show("You should to fill at least Id field!");
                }
                else
                {
                    int id = Int32.Parse(idTextBox.Text);
                    string login = loginTextBox.Text;
                    string password = passwordTextBox.Text;
                    string name = nameTextBox.Text;
                    string surname = surnameTextBox.Text;
                    
                    user user = usersContext.users.First(i => i.Id == id);

                    if (login != string.Empty)
                    {
                        user.Login = login;
                    }
                    if (password != string.Empty)
                    {
                        user.UserPassword = password;
                    }
                    if (name != string.Empty)
                    {
                        user.UserName = name;
                    }
                    if (surname != string.Empty)
                    {
                        user.SurName = surname;
                    }
                    usersContext.SaveChanges();

                    //write to log
                    MainWindow owner = (MainWindow)this.Owner;
                    owner.WriteToLog(string.Format(DateTime.Now + " User {0} Updated" + Environment.NewLine, user.Login));
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "::" + ex.InnerException);
            }
        }
    }
}
