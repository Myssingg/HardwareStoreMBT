using System;
using System.Linq;
using System.Windows;

namespace HardwareStoreMBT
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow() { InitializeComponent(); }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var surname = txtSurname.Text.Trim();
            var name = txtName.Text.Trim();
            var patronymic = txtPatronymic.Text.Trim();
            var phone = txtPhone.Text.Trim();
            var email = txtEmail.Text.Trim();
            var pass = txtPassword.Password;
            var address = txtAddress.Text.Trim();

            if (string.IsNullOrEmpty(surname) 
                || string.IsNullOrEmpty(name) 
                || string.IsNullOrEmpty(phone) 
                || string.IsNullOrEmpty(email) 
                || string.IsNullOrEmpty(pass))
            {
                tbError.Text = "Заполните обязательные поля (Фамилия, Имя, Телефон, Email, Пароль)";
                return;
            }

            try
            {
                using (var db = new HardwareStoreMBTEntities())
                {
                    // Проверка уникальности email (на всякий случай, триггер тоже проверит)
                    if (db.clients.Any(c => c.email == email))
                    {
                        tbError.Text = "Клиент с таким email уже существует";
                        return;
                    }

                    var client = new clients
                    {
                        surname = surname,
                        name = name,
                        patronymic = string.IsNullOrEmpty(patronymic) ? null : patronymic,
                        phoneNumber = phone,
                        email = email,
                        passwordHash = App.ComputeSha256(pass),
                        address = string.IsNullOrEmpty(address) ? null : address
                    };

                    db.clients.Add(client);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация успешна! Войдите под своим email.", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    new LoginWindow(LoginWindow.AuthMode.Client).Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                tbError.Text = "Ошибка: " + ex.Message;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow(LoginWindow.AuthMode.Client).Show();
            Close();
        }
    }
}