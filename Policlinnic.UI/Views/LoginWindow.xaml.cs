using System.Windows;
using System.Windows.Input;
using Policlinnic.UI.Views.Pages;

namespace Policlinnic.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim().ToLower();

            // Простая имитация проверки ролей (Критерий 6.4)
            MainWindow mainWindow = new MainWindow();

            if (login == "admin")
            {
                mainWindow.MainFrame.Navigate(new AdminPage());
            }
            else if (login == "doctor")
            {
                mainWindow.MainFrame.Navigate(new DoctorPage());
            }
            else if (!string.IsNullOrEmpty(login))
            {
                mainWindow.MainFrame.Navigate(new PatientPage());
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            mainWindow.Show();
            this.Close();
        }
    }
}