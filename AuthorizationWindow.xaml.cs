using System;
using System.Linq;
using System.Windows;

namespace ShoeStore_Bondarev
{
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new ShoeStore_BondarevEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.Логин == login && u.Пароль == password);

                    if (user != null)
                    {
                        // пдтяжка ролей вручную
                        var role = context.Roles.FirstOrDefault(r => r.ID_Роли == user.Id_Роли);
                        string roleName = role != null ? role.Наименование_Роли : "Неизвестно";
                        int roleId = role != null ? role.ID_Роли : 0;

                        MessageBox.Show($"Добро пожаловать, {user.ФИО}!\nВаша роль: {roleName}",
                                        "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);

                        // роль айди в мейнменювиндов
                        MainMenuWindow mainWindow = new MainMenuWindow(roleName, user.ФИО, roleId);
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль.", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к БД: " + ex.Message, "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            // гость
            MainMenuWindow mainWindow = new MainMenuWindow("Гость", "Гость", 4);
            mainWindow.Show();
            this.Close();
        }
    }
}