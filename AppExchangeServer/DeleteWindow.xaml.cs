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
    /// Логика взаимодействия для DeleteWindow.xaml
    /// </summary>
    public partial class DeleteWindow : Window
    {

        private UsersContext usersContext;

        public DeleteWindow()
        {
            InitializeComponent();
        }

        private void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (usersContext == null)
                {
                    usersContext = new UsersContext();
                }

                int id = Int32.Parse(idTextBox.Text);
                user user = usersContext.users.First(i => i.Id == id);
                usersContext.users.Remove(user);
                usersContext.SaveChanges();

                //write to log
                MainWindow owner = (MainWindow)this.Owner;
                owner.WriteToLog(string.Format(DateTime.Now + " User {0} Deleted" + Environment.NewLine, user.Login));
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
