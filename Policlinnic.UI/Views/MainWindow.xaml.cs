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

        // --- ЛОГИКА ДОСТУПА (Кнопки) ---
        private void ApplyAccessControl()
        {
            // 1. АДМИНИСТРАТОР
            if (_currentUser.IDRole == 1)
            {
                RbUsers.IsChecked = true;
                TxtPageTitle.Text = "Пользователи";
                // RbPatients больше нет, скрывать нечего
                MainFrame.Navigate(new UsersPage());
            }
            // 2. ВРАЧ
            else if (_currentUser.IDRole == 2)
            {
                // Скрываем админское
                RbUsers.Visibility = Visibility.Collapsed;
                RbArchive.Visibility = Visibility.Collapsed;
                RbDictionaries.Visibility = Visibility.Collapsed;

                // Меняем названия кнопок под врача
                RbAppointments.Content = "Мои приёмы";
                RbSickLeaves.Content = "Выписанные больничные";
                RbDiagnoses.Content = "Мои диагнозы";

                // ТАК КАК "ПАЦИЕНТЫ" УДАЛЕНЫ, СТАВИМ ПО УМОЛЧАНИЮ "ПРИЕМЫ"
                RbAppointments.IsChecked = true;
                TxtPageTitle.Text = "Мои приёмы";
                // MainFrame.Navigate(new AppointmentsPage());
            }
            // 3. ПАЦИЕНТ
            else if (_currentUser.IDRole == 3)
            {
                // Скрываем лишнее
                RbUsers.Visibility = Visibility.Collapsed;
                // RbPatients удален
                RbReports.Visibility = Visibility.Collapsed;
                RbArchive.Visibility = Visibility.Collapsed;
                RbStatistics.Visibility = Visibility.Collapsed;
                RbDictionaries.Visibility = Visibility.Collapsed;

                // Меняем названия под пациента
                RbAppointments.Content = "Мои записи";
                RbSickLeaves.Content = "Мои больничные";
                RbDiagnoses.Content = "История болезней";
                RbTreatments.Content = "Моё лечение";

                RbAppointments.IsChecked = true;
                TxtPageTitle.Text = "Мои записи";
                // MainFrame.Navigate(new PatientAppointmentsPage(_currentUser.Id));
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
                    case "RbAppointments":
                        // Логика может отличаться для ролей
                        // if (_currentUser.IDRole == 3) MainFrame.Navigate(new PatientAppointmentsPage(_currentUser.Id));
                        // else MainFrame.Navigate(new AppointmentsPage());
                        break;
                    case "RbSickLeaves":
                        MainFrame.Navigate(new SickLeavesPage(_currentUser));
                        break;
                    case "RbDiagnoses":
                        // MainFrame.Navigate(new DiagnosesPage());
                        break;
                    case "RbTreatments":
                        // MainFrame.Navigate(new TreatmentsPage());
                        break;
                    case "RbExamPlans":
                        // MainFrame.Navigate(new ExamPlansPage());
                        break;
                    case "RbDictionaries":
                        // MainFrame.Navigate(new DictionariesPage());
                        break;
                    case "RbReports":
                        // MainFrame.Navigate(new ReportsPage());
                        break;
                    case "RbArchive":
                        // MainFrame.Navigate(new ArchivePage());
                        break;
                    case "RbStatistics":
                        // MainFrame.Navigate(new StatisticsPage());
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
    }
}