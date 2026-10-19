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

namespace HardwareStoreMBT
{
    /// <summary>
    /// Логика взаимодействия для RoleSelectWindow.xaml
    /// </summary>
    public partial class RoleSelectWindow : Window
    {
        public RoleSelectWindow()
        {
            InitializeComponent();
        }

        private void btnClient_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow(LoginWindow.AuthMode.Client).Show();
            Close();
        }

        private void btnEmployee_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow(LoginWindow.AuthMode.Employee).Show();
            Close();
        }
    }
}

