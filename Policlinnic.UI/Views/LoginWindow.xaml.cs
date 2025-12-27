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
            // Инициализируем сервис, который умеет общаться с БД и хешировать пароли
            _authService = new AuthService();
        }

        // 1. Логика перетаскивания окна (так как WindowStyle="None")
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
                // Вызываем наш BLL сервис. Он пойдет в БД, возьмет хеш и сравнит.
                User user = _authService.Login(login, password);

                if (user != null)
                {
                    // УСПЕХ! Открываем главное окно
                    MainWindow mainWindow = new MainWindow();

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
                // Если база недоступна или упала ошибка подключения
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}", "Критическая ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Вспомогательный метод для переключения страниц по ролям
        private void ConfigureWindowForRole(MainWindow window, int roleId)
        {
            switch (roleId)
            {
                case 1: // Админ
                    window.MainFrame.Navigate(new AdminPage());
                    break;
                case 2: // Врач
                    window.MainFrame.Navigate(new DoctorPage());
                    break;
                case 3: // Пациент
                    window.MainFrame.Navigate(new PatientPage());
                    break;
                default:
                    MessageBox.Show("Роль пользователя не определена системой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
    }
}