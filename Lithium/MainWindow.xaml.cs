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

namespace Lithium
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (CheckUserLogin(UsernameBox.Text, PasswordBox.Password))
            {
                Window1 AppWindow = new Window1();
                AppWindow.Show();
                this.Close();
            }
        }

        private readonly string Password = "1234";
        private readonly string Username = "BlackStrider";

        bool CheckUserLogin(string User, string Pass)
        {
            if (Equals(User, Username) && Equals(Pass, Password))
                return true;
            else
                return false;
        }
    }
}
