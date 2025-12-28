using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;
using Policlinnic.UI.Views;

namespace Policlinnic.UI.Views.Pages
{
    public partial class UsersPage : Page
    {
        private readonly UserRepository _userRepository;
        private List<UserView> _allUsers;

        public UsersPage()
        {
            InitializeComponent();
            _userRepository = new UserRepository();

            // Загружаем данные
            LoadData();

            // Подписываемся на событие загрузки страницы, чтобы установить фокус
            this.Loaded += UsersPage_Loaded;
        }

        // Требование 3.5: Установка фокуса на первый элемент при открытии
        private void UsersPage_Loaded(object sender, RoutedEventArgs e)
        {
            TxtSearch.Focus();
        }

        private void LoadData()
        {
            try
            {
                _allUsers = _userRepository.GetAllUsers();
                ApplyFilters();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void ApplyFilters()
        {
            if (_allUsers == null) return;

            var filtered = _allUsers.AsEnumerable();

            // Поиск
            string searchText = TxtSearch.Text.ToLower().Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(u =>
                    u.FIO.ToLower().Contains(searchText) ||
                    u.Login.ToLower().Contains(searchText) ||
                    (u.Phone != null && u.Phone.Contains(searchText)));
            }

            // Фильтр по роли
            if (CmbRoleFilter.SelectedItem is ComboBoxItem selectedItem)
            {
                string role = selectedItem.Content.ToString();
                if (role != "Все роли")
                {
                    filtered = filtered.Where(u => u.RoleName == role);
                }
            }

            UsersGrid.ItemsSource = filtered.ToList();
        }

        // --- СОБЫТИЯ ---

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbRoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        // КНОПКА ДОБАВИТЬ
        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            // Открываем модальное окно создания
            var editWindow = new UserEditWindow();
            editWindow.ShowDialog(); // Ждем закрытия

            // Требование 3.8: Автоматическое обновление после действия
            if (editWindow.IsSuccess)
            {
                LoadData();
            }
        }

        // КНОПКА РЕДАКТИРОВАТЬ (Карандаш в строке)
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Получаем пользователя из строки, где нажали кнопку
            var user = ((FrameworkElement)sender).DataContext as UserView;

            if (user != null)
            {
                // Открываем окно в режиме редактирования
                var editWindow = new UserEditWindow(user);
                editWindow.ShowDialog();

                // Авто-обновление, если сохранили изменения
                if (editWindow.IsSuccess)
                {
                    LoadData();
                }
            }
        }

        // КНОПКА УДАЛИТЬ (Крестик в строке)
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var user = ((FrameworkElement)sender).DataContext as UserView;
            if (user == null) return;

            // Требование 3.6: Защита (спрашиваем подтверждение)
            if (MessageBox.Show($"Удалить пользователя {user.FIO}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // TODO: Реализовать _userRepository.DeleteUser(user.Id)
                    // _userRepository.DeleteUser(user.Id);

                    MessageBox.Show("Пользователь удален (логика в репо нужна)");

                    // Автоматическое обновление
                    LoadData();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Ошибка удаления: " + ex.Message);
                }
            }
        }
    }
}