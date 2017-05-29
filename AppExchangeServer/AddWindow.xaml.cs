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
    /// Логика взаимодействия для AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {

        private UsersContext usersContext;

        public AddWindow()
        {
            InitializeComponent();
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (usersContext == null)
            {
                usersContext = new UsersContext();
            }

            try
            {
                if ((idTextBox.Text == string.Empty) || (loginTextBox.Text == string.Empty)
                    || (passwordTextBox.Text == string.Empty) || (nameTextBox.Text == string.Empty)
                    || (surnameTextBox.Text == string.Empty))
                {
                    MessageBox.Show("You should to fill all the fields!");
                }
                else
                {
                    int id = Int32.Parse(idTextBox.Text);
                    string login = loginTextBox.Text;
                    string password = passwordTextBox.Text;
                    string name = nameTextBox.Text;
                    string surname = surnameTextBox.Text;

                    user user = new user() { Id = id, Login = login, UserPassword = password, UserName = name, SurName = surname};
                    //add to bd
                    usersContext.users.Add(user);
                    usersContext.SaveChanges();

                    //write to log
                    MainWindow owner = (MainWindow)this.Owner;
                    owner.WriteToLog(string.Format(DateTime.Now + " User {0} Added" + Environment.NewLine, user.Login));
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
