using System.Windows;
using System.Windows.Input;
using Policlinnic.BLL.Services;
using Policlinnic.Domain.Entities;
using Policlinnic.UI.Views.Pages;

namespace Policlinnic.UI.Views
{
    public partial class LoginWindow : Window
    {
        // Поле для сервиса авторизации
        private readonly AuthService _authService;

        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // 2. Логика кнопки "Крестик" (Закрыть приложение)
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // 3. Логика кнопки "ВХОД"
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Берем данные из полей, которые ты назвала в XAML
            string login = LoginBox.Text.Trim();
            string password = PassBox.Password.Trim(); // Важно: в XAML это PassBox

            // Простая валидация на пустоту
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                User user = _authService.Login(login, password);

                if (user != null)
                {
                    MainWindow mainWindow = new MainWindow(user);

                    // Настраиваем вид в зависимости от роли
                    ConfigureWindowForRole(mainWindow, user.IDRole);

                    mainWindow.Show();
                    this.Close(); // Закрываем окно авторизации
                }
                else
                {
                    // Ошибка: Пользователь не найден или пароль не подошел
                    MessageBox.Show("Неверный логин или пароль.", "Ошибка входа",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}", "Критическая ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ConfigureWindowForRole(MainWindow window, int roleId)
        {
            switch (roleId)
            {
                case 1: 
                    window.MainFrame.Navigate(new Policlinnic.UI.Views.Pages.UsersPage());
                    break;

                case 2:
                    window.MainFrame.Navigate(new Policlinnic.UI.Views.Pages.AppointmentsPage());
                    break;

                case 3: 
                    window.MainFrame.Navigate(new Policlinnic.UI.Views.Pages.AppointmentsPage());
                    break;

                default:
                    MessageBox.Show("Роль пользователя не определена системой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
    }
}