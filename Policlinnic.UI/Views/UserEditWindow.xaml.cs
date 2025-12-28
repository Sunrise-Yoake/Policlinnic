using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;
using Policlinnic.BLL.Services;

// ВАЖНО: Пространство имен должно быть Policlinnic.UI.Views (как в x:Class)
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
            if (string.IsNullOrWhiteSpace(TxtLogin.Text) ||
                string.IsNullOrWhiteSpace(TxtFIO.Text) ||
                DpBirth.SelectedDate == null)
            {
                MessageBox.Show("Заполните обязательные поля (*)!");
                return;
            }

            try
            {
                string role = (CmbRole.SelectedItem as ComboBoxItem).Content.ToString();

                var user = new UserView
                {
                    Id = _userToEdit != null ? _userToEdit.Id : 0,
                    Login = TxtLogin.Text,
                    Password = _userToEdit == null ? PasswordHasher.Hash(TxtPassword.Text) : null,
                    Phone = TxtPhone.Text,
                    RoleName = role,
                    FIO = TxtFIO.Text,
                    DateOfBirth = DpBirth.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    Gender = (CmbGender.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Мужской",
                    Address = TxtAddress.Text,
                };

                int.TryParse(TxtExperience.Text, out int exp);
                user.Experience = exp;

                if (role == "Врач" && CmbSpecialization.SelectedValue != null)
                {
                    user.IDSpecialization = (int)CmbSpecialization.SelectedValue;
                }

                if (_userToEdit == null)
                {
                    _repository.AddUserWithProfile(user);
                }
                else
                {
                    MessageBox.Show("Редактирование пока работает только визуально.");
                }

                IsSuccess = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}