using System;
using System.Windows;
using System.Windows.Controls;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;

namespace Policlinnic.UI.Views.Pages
{
    public partial class AppointmentsPage : Page
    {
        private readonly AppointmentRepository _repo;
        private int _userRole;
        private int _userId;

        public AppointmentsPage(User currentUser)
        {
            InitializeComponent();
            _repo = new AppointmentRepository();
            _userRole = currentUser.IDRole;
            _userId = currentUser.Id;
            SetupUI();
            LoadMainGrid();
        }

        private void SetupUI()
        {
            // Грузим общие данные для поиска
            CmbSpec.ItemsSource = _repo.GetSpecs();

            if (_userRole == 3) // ПАЦИЕНТ
            {
                PanelPatient.Visibility = Visibility.Visible;
                PanelAdmin.Visibility = Visibility.Collapsed;
                BlockPatientSelect.Visibility = Visibility.Collapsed; // Скрываем выбор пациента

                TxtGridHeader.Text = "Мои записи";
                ColPatient.Visibility = Visibility.Collapsed;
                BtnDelete.Visibility = Visibility.Collapsed;
            }
            else // ВРАЧ или АДМИН
            {
                // Врач видит ОБЕ панели: может и записать кого-то, и управлять
                PanelPatient.Visibility = Visibility.Visible;

                // Выбор пациента доступен врачу
                BlockPatientSelect.Visibility = Visibility.Visible;
                CmbTargetPatient.ItemsSource = _repo.GetAllPatients(); // Грузим пациентов

                // Панель админа: Врач видит (если нужно), Админ видит точно
                PanelAdmin.Visibility = Visibility.Visible;

                TxtGridHeader.Text = "Все записи";
                ColPatient.Visibility = Visibility.Visible;
                BtnDelete.Visibility = Visibility.Visible;

                CmbAdminDoctor.ItemsSource = _repo.GetAllDoctors();
            }
        }

        private void LoadMainGrid()
        {
            try
            {
                if (_userRole == 3)
                {
                    // Пациент: Мои записи
                    GridMain.ItemsSource = _repo.GetByPatient(_userId);
                }
                else
                {
                    // Админ/Врач: Все записи (передаем null, чтобы получить всех)
                    GridMain.ItemsSource = _repo.GetByDoctor(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        // --- ЛОГИКА ЗАПИСИ ---

        private void CmbSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbSpec.SelectedValue is int specId)
            {
                CmbDoctor.ItemsSource = _repo.GetDoctorsBySpec(specId);
                GridFreeSlots.ItemsSource = null;
            }
        }

        // Свободные слоты
        private void CmbDoctor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbDoctor.SelectedValue is int docId)
            {
                // Ищем свободные слоты конкретного врача
                GridFreeSlots.ItemsSource = _repo.GetFreeSlots(doctorId: docId);
            }
        }

        private void BtnBook_Click(object sender, RoutedEventArgs e)
        {
            var slot = (sender as Button)?.DataContext as AppointmentItem;
            if (slot == null) return;

            // ОПРЕДЕЛЯЕМ, КОГО ЗАПИСЫВАЕМ
            int patientToBookId = _userId; // По умолчанию - себя

            if (_userRole != 3) // Если я Врач или Админ
            {
                if (CmbTargetPatient.SelectedValue == null)
                {
                    MessageBox.Show("Выберите пациента из списка 'Кого записываем'!");
                    return;
                }
                patientToBookId = (int)CmbTargetPatient.SelectedValue;
            }

            if (MessageBox.Show($"Записать пациента (ID {patientToBookId}) на {slot.DisplayDate}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _repo.BookSlot(slot.Id, patientToBookId);
                    MessageBox.Show("Запись создана!");
                    LoadMainGrid();

                    // Обновляем список свободных слотов
                    if (CmbDoctor.SelectedValue is int docId)
                        GridFreeSlots.ItemsSource = _repo.GetFreeSlots(doctorId: docId);
                }
                catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            }
        }

        // ... Остальные методы (BtnAddSlot_Click, BtnCancelDay_Click, BtnDelete_Click) без изменений ...
        private void BtnAddSlot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbAdminDoctor.SelectedValue == null) throw new Exception("Выберите врача");
                if (!DateTime.TryParse(TxtNewDate.Text, out DateTime date)) throw new Exception("Неверный формат даты");

                int docId = (int)CmbAdminDoctor.SelectedValue;
                _repo.Add(docId, date, TxtNewCabinet.Text);
                MessageBox.Show("Слот добавлен!");
                LoadMainGrid();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnCancelDay_Click(object sender, RoutedEventArgs e)
        {
            if (CmbAdminDoctor.SelectedValue == null || DpCancelDay.SelectedDate == null) return;
            int docId = (int)CmbAdminDoctor.SelectedValue;
            int count = _repo.CancelDoctorDay(docId, DpCancelDay.SelectedDate.Value);
            MessageBox.Show($"Отменено: {count}");
            LoadMainGrid();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = GridMain.SelectedItem as AppointmentItem;
            if (selected != null)
            {
                _repo.Delete(selected.Id);
                LoadMainGrid();
            }
        }
    }
}