using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;
using Policlinnic.BLL.Services;

namespace Policlinnic.UI.Views
{
    public partial class UserEditWindow : Window
    {
        private readonly UserRepository _repository;
        private UserView _userToEdit;
        public bool IsSuccess { get; private set; } = false;

        // Конструктор создания
        public UserEditWindow()
        {
            InitializeComponent();
            _repository = new UserRepository();
            LoadSpecializations();
            Title = "Новый пользователь";
        }

        // Конструктор редактирования
        public UserEditWindow(UserView user) : this()
        {
            _userToEdit = user;
            Title = "Редактирование: " + user.FIO;

            // Заполнение полей (имена из XAML)
            TxtLogin.Text = user.Login;
            TxtPassword.IsEnabled = false;
            TxtPhone.Text = user.Phone;
            TxtFIO.Text = user.FIO;
            TxtAddress.Text = user.Address;
            TxtExperience.Text = user.Experience?.ToString();

            if (DateTime.TryParse(user.DateOfBirth, out DateTime dob))
                DpBirth.SelectedDate = dob;

            // Выбор пола
            foreach (ComboBoxItem item in CmbGender.Items)
            {
                if (item.Content.ToString() == user.Gender)
                {
                    CmbGender.SelectedItem = item;
                    break;
                }
            }

            // Выбор роли
            foreach (ComboBoxItem item in CmbRole.Items)
            {
                if (item.Content.ToString() == user.RoleName)
                {
                    CmbRole.SelectedItem = item;
                    break;
                }
            }
            CmbRole.IsEnabled = false;
        }

        private void LoadSpecializations()
        {
            try
            {
                var specs = _repository.GetSpecializations();
                CmbSpecialization.ItemsSource = specs;
                if (specs.Count > 0) CmbSpecialization.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки специализаций: " + ex.Message);
            }
        }

        private void CmbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelPatient == null || PanelEmployee == null || PanelDoctorSpec == null) return;

            var item = CmbRole.SelectedItem as ComboBoxItem;
            if (item == null) return;

            string role = item.Content.ToString();

            // Сброс видимости
            PanelPatient.Visibility = Visibility.Collapsed;
            PanelEmployee.Visibility = Visibility.Collapsed;
            PanelDoctorSpec.Visibility = Visibility.Collapsed;

            // Логика отображения
            if (role == "Пациент")
            {
                PanelPatient.Visibility = Visibility.Visible;
            }
            else if (role == "Админ")
            {
                PanelEmployee.Visibility = Visibility.Visible;
            }
            else if (role == "Врач")
            {
                PanelEmployee.Visibility = Visibility.Visible;
                PanelDoctorSpec.Visibility = Visibility.Visible;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtLogin.Text) || DpBirth.SelectedDate == null || string.IsNullOrWhiteSpace(TxtFIO.Text))
            {
                MessageBox.Show("Заполните обязательные поля (Логин, ФИО, Дата рождения)!");
                return;
            }

            try
            {
                var user = new UserView
                {
                    Id = _userToEdit?.Id ?? 0,
                    Login = TxtLogin.Text,
                    Phone = TxtPhone.Text,
                    RoleName = (CmbRole.SelectedItem as ComboBoxItem).Content.ToString(),
                    FIO = TxtFIO.Text,
                    DateOfBirth = DpBirth.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    Gender = (CmbGender.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Address = TxtAddress.Text,
                    Experience = int.TryParse(TxtExperience.Text, out int exp) ? exp : (int?)null,
                    IDSpecialization = CmbSpecialization.SelectedValue as int?
                };

                if (_userToEdit == null) // НОВЫЙ
                {
                    if (string.IsNullOrWhiteSpace(TxtPassword.Text)) { MessageBox.Show("Введите пароль!"); return; }
                    user.Password = TxtPassword.Text; // Добавь хеширование если нужно
                    _repository.AddUserWithProfile(user);
                }
                else // РЕДАКТИРОВАНИЕ
                {
                    _repository.UpdateUserWithProfile(user);
                }

                IsSuccess = true;
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        // Метод удаления теперь не вызывает репозиторий, а сообщает о запрете (или кнопка просто удаляется из XAML)
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Удаление пользователей запрещено правилами системы. Используйте архивацию или триггер БД.",
                            "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Кнопка редактирования (остается без изменений)
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var user = ((FrameworkElement)sender).DataContext as UserView;
            if (user != null)
            {
                var editWindow = new UserEditWindow(user);
                if (editWindow.ShowDialog() == true || editWindow.IsSuccess)
                {
                    _repository.UpdateUserWithProfile(user);
                }
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}