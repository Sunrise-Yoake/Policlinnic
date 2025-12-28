using System.Windows;
using System.Windows.Controls;
using Policlinnic.Domain.Entities;
using Policlinnic.DAL.Repositories; // Подключаем пространство имен репозиториев
using Policlinnic.UI.Views;
using Policlinnic.UI.Views.Pages;

namespace Policlinnic.UI
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private readonly AdminRepository _adminRepository; // 1. Объявляем переменную репозитория

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

            // 3. Логика для АДМИНИСТРАТОРА (IDRole = 1)
            if (_currentUser.IDRole == 1)
            {
                TxtRoleName.Text = "Администратор системы";

                // Идем в базу за подробностями (ищем по ID пользователя)
                Admin adminDetails = _adminRepository.GetAdminById(_currentUser.Id);

                if (adminDetails != null)
                {
                    // БЕРЕМ ТОЛЬКО ПЕРВЫЕ ДВА СЛОВА (Фамилия и Имя)
                    string fullName = adminDetails.FullName; // "Караханова Ксения Сеттаровна"
                    var parts = fullName.Split(' '); // Разбиваем по пробелам в массив

                    if (parts.Length >= 2)
                    {
                        // Собираем обратно только первые две части
                        TxtUserLogin.Text = $"{parts[0]} {parts[1]}";
                    }
                    else
                    {
                        // Если вдруг имя короткое (только одно слово), выводим как есть
                        TxtUserLogin.Text = fullName;
                    }
                }
            }
            // 4. Логика для ВРАЧА (IDRole = 2)
            else if (_currentUser.IDRole == 2)
            {
                TxtRoleName.Text = "Врач";
                // В будущем здесь будет:
                // DoctorRepository docRepo = new DoctorRepository();
                // var doc = docRepo.GetDoctorById(_currentUser.Id);
                // if (doc != null) TxtUserLogin.Text = doc.FullName;
            }
            // 5. Логика для ПАЦИЕНТА (IDRole = 3)
            else if (_currentUser.IDRole == 3)
            {
                TxtRoleName.Text = "Пациент";
                // Аналогично для пациента
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
                RbPatients.Visibility = Visibility.Collapsed;
                MainFrame.Navigate(new UsersPage()); 
            }
            // 2. ВРАЧ
            else if (_currentUser.IDRole == 2)
            {
                // Скрываем лишнее
                RbUsers.Visibility = Visibility.Collapsed;
                RbArchive.Visibility = Visibility.Collapsed;
                RbDictionaries.Visibility = Visibility.Collapsed;

                // Меняем названия кнопок под врача
                RbAppointments.Content = "Мои приёмы";
                RbSickLeaves.Content = "Выписанные больничные";
                RbDiagnoses.Content = "Мои диагнозы";

                RbPatients.IsChecked = true;
                TxtPageTitle.Text = "Пациенты";
                // MainFrame.Navigate(new PatientsPage());
            }
            // 3. ПАЦИЕНТ
            else if (_currentUser.IDRole == 3)
            {
                // Скрываем админское и врачебное
                RbUsers.Visibility = Visibility.Collapsed;
                RbPatients.Visibility = Visibility.Collapsed;
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
                // Берем заголовок прямо с кнопки (с учетом изменений для врача/пациента)
                TxtPageTitle.Text = rb.Content.ToString();

                switch (rb.Name)
                {
                    case "RbUsers":
                        MainFrame.Navigate(new UsersPage());
                        break;
                    case "RbPatients":
                        // MainFrame.Navigate(new PatientsPage());
                        break;
                    case "RbAppointments":
                        // Логика может отличаться для ролей
                        // if (_currentUser.IDRole == 3) MainFrame.Navigate(new PatientAppointmentsPage(_currentUser.Id));
                        // else MainFrame.Navigate(new AppointmentsPage());
                        break;
                    case "RbSickLeaves":
                        // MainFrame.Navigate(new SickLeavesPage());
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