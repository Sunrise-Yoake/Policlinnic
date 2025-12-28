using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;

namespace Policlinnic.UI.Views.Pages
{
    public partial class PatientsPage : Page
    {
        private UserRepository _repo = new UserRepository();
        private List<UserFullInfo> _allPatients;

        public PatientsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Берем ВСЕХ, но оставляем только ПАЦИЕНТОВ
            var allUsers = _repo.GetUsersFromView();
            _allPatients = allUsers.Where(u => u.RoleName == "Пациент").ToList();

            PatientsGrid.ItemsSource = _allPatients;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filter = TxtSearch.Text.ToLower();
            PatientsGrid.ItemsSource = _allPatients.Where(p =>
                p.FullName.ToLower().Contains(filter) ||
                p.Phone.Contains(filter) ||
                (p.PatientAddress != null && p.PatientAddress.ToLower().Contains(filter))
            ).ToList();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var link = sender as System.Windows.Documents.Hyperlink;
            var user = link.DataContext as UserFullInfo;
            if (user != null)
            {
                new EditUserWindow(user).ShowDialog();
                LoadData(); // Обновить таблицу после редактирования
            }
        }
    }
}