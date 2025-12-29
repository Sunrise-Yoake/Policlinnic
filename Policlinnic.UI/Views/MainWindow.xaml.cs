using System.Windows;
using System.Windows.Controls;
using Policlinnic.Domain.Entities;
using Policlinnic.DAL.Repositories;
using Policlinnic.UI.Views;
using Policlinnic.UI.Views.Pages;

namespace Policlinnic.UI
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private readonly AdminRepository _adminRepository;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _adminRepository = new AdminRepository();

            InitUserInterface();
            ApplyAccessControl();
        }

        private void InitUserInterface()
        {
            TxtUserLogin.Text = _currentUser.Login;

            // 1. АДМИНИСТРАТОР
            if (_currentUser.IDRole == 1)
            {
                TxtRoleName.Text = "Администратор системы";

                if (_adminRepository != null)
                {
                    try
                    {
                        Admin adminDetails = _adminRepository.GetAdminById(_currentUser.Id);
                        if (adminDetails != null)
                        {
                            string fullName = adminDetails.FullName;
                            var parts = fullName.Split(' ');
                            if (parts.Length >= 2)
                                TxtUserLogin.Text = $"{parts[0]} {parts[1]}";
                            else
                                TxtUserLogin.Text = fullName;
                        }
                    }
                    catch {  }
                }
            }
            // 2. ВРАЧ
            else if (_currentUser.IDRole == 2)
            {
                TxtRoleName.Text = "Врач";
            }
            // 3. ПАЦИЕНТ
            else if (_currentUser.IDRole == 3)
            {
                TxtRoleName.Text = "Пациент";
            }
            else
            {
                TxtRoleName.Text = "Пользователь";
            }
        }

        // --- ЛОГИКА ДОСТУПА (Скрытие кнопок) ---
        private void ApplyAccessControl()
        {
            // 1. АДМИНИСТРАТОР
            if (_currentUser.IDRole == 1)
            {
                RbUsers.IsChecked = true;
                TxtPageTitle.Text = "Пользователи";
                MainFrame.Navigate(new UsersPage());
            }
            // 2. ВРАЧ
            else if (_currentUser.IDRole == 2)
            {
                // Скрываем кнопки админа
                RbUsers.Visibility = Visibility.Collapsed;
                RbStatistics.Visibility = Visibility.Collapsed;
                RbArchive.Visibility = Visibility.Collapsed;
                RbReports.Visibility = Visibility.Collapsed;
                RbDictionaries.Visibility = Visibility.Collapsed; 

                RbAppointments.IsChecked = true;
                MainFrame.Navigate(new AppointmentsPage(_currentUser));
            }
            // 3. ПАЦИЕНТ
            else if (_currentUser.IDRole == 3)
            {
                // Скрываем лишнее
                RbUsers.Visibility = Visibility.Collapsed;
                RbReports.Visibility = Visibility.Collapsed;
                RbArchive.Visibility = Visibility.Collapsed;
                RbStatistics.Visibility = Visibility.Collapsed;
                RbDictionaries.Visibility = Visibility.Collapsed;

                // Меняем названия
                RbAppointments.Content = "Мои записи";
                RbSickLeaves.Content = "Мои больничные";
                RbTreatments.Content = "Моё лечение";

                RbAppointments.IsChecked = true;
                TxtPageTitle.Text = "Мои записи";
                MainFrame.Navigate(new AppointmentsPage(_currentUser)); 
            }
        }

        // --- НАВИГАЦИЯ ---
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                TxtPageTitle.Text = rb.Content.ToString();

                switch (rb.Name)
                {
                    case "RbUsers":
                        MainFrame.Navigate(new UsersPage());
                        break;

                    case "RbDictionaries":
                        MainFrame.Navigate(new DictionariesPage());
                        break;

                    case "RbSickLeaves":
                        MainFrame.Navigate(new SickLeavesPage(_currentUser));
                        break;
                    
                    case "RbReports":
                        MainFrame.Navigate(new ReportsPage());
                        break;

                    case "RbStatistics":
                        MainFrame.Navigate(new StatisticsPage());
                        break;

                    case "RbAppointments":
                        // Передаем _currentUser внутрь страницы
                        MainFrame.Navigate(new AppointmentsPage(_currentUser));
                        break;

                }
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void RbDictionaries_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}