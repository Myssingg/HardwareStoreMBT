using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace HardwareStoreMBT
{
    public partial class LoginWindow : Window
    {
        public enum AuthMode { Client, Employee }
        private AuthMode _mode;

        public LoginWindow(AuthMode mode)
        {
            InitializeComponent();
            _mode = mode;
            if (mode == AuthMode.Client)
            {
                tbTitle.Text = "Вход для клиента";
                lblLogin.Text = "Email";
            }
            else
            {
                tbTitle.Text = "Вход для сотрудника";
                lblLogin.Text = "Логин";
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = txtLogin.Text.Trim();
            var pass = txtPassword.Password;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
            {
                tbError.Text = "Заполните все поля"; return;
            }

            var hash = App.ComputeSha256(pass);

            try
            {
                using (var db = new HardwareStoreMBTEntities())
                {
                    if (_mode == AuthMode.Client)
                    {
                        var client = db.clients.FirstOrDefault(c => c.email == login && c.passwordHash == hash);
                        if (client == null) { tbError.Text = "Неверный email или пароль"; return; }

                        App.Current.Properties["UserRole"] = "Client";
                        App.Current.Properties["CurrentClient"] = client;
                        new ClientWindow().Show();
                    }
                    else
                    {
                        var emp = db.employees.Include(x => x.roles).FirstOrDefault(x => x.login == login && x.passwordHash == hash);
                        if (emp == null) { tbError.Text = "Неверный логин или пароль"; return; }

                        var roleName = emp.roles?.name?.ToLower() ?? "";
                        App.Current.Properties["UserRole"] = roleName;
                        App.Current.Properties["CurrentEmployee"] = emp;

                        if (roleName.Contains("админ") || roleName.Contains("admin"))
                            new AdminWindow().Show();
                        else if (roleName.Contains("менеджер") || roleName.Contains("manager"))
                            new ManagerWindow().Show();
                        else { tbError.Text = "Роль не определена"; return; }
                    }
                    Close();
                }
            }
            catch (Exception ex) { tbError.Text = "Ошибка: " + ex.Message; }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == AuthMode.Client)
            {
                new RegisterWindow().Show();
                Close();
            }
            else
            {
                tbError.Text = "Регистрация доступна только для клиентов";
            }
        }
    }
}