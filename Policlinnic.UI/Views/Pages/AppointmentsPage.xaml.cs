using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;

namespace Policlinnic.UI.Views.Pages
{
    public partial class AppointmentsPage : Page
    {
        private readonly AppointmentRepository _repo;
        private readonly ReportRepository _reportRepo;
        private User _currentUser;
        private int? _editingSlotId = null;
        private int? _currentPatientId = null; 

        public AppointmentsPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _repo = new AppointmentRepository();
            _reportRepo = new ReportRepository();

            SetupUI();
            LoadMainGrid();
        }

        private void SetupUI()
        {
            // Справочники
            CmbSpec.ItemsSource = _repo.GetSpecs();
            CmbAdminDoctor.ItemsSource = _repo.GetAllDoctors();
            CmbFilterDoctor.ItemsSource = _repo.GetAllDoctors(); // Для фильтра сверху

            if (_currentUser.IDRole == 3) // ПАЦИЕНТ
            {
                TxtGridHeader.Text = "Мои записи";
                PanelTopFilter.Visibility = Visibility.Collapsed; // Пациенту фильтр не нужен
                PanelAdmin.Visibility = Visibility.Collapsed;
                ColActions.Visibility = Visibility.Collapsed;
                ColPatient.Visibility = Visibility.Collapsed;
                BtnCleanOld.Visibility = Visibility.Collapsed;

                // Нижняя панель видна
                PanelFreeSlots.Visibility = Visibility.Visible;
            }
            else // АДМИН (1) или ВРАЧ (2)
            {
                TxtGridHeader.Text = "Все приёмы (Администрирование)";
                PanelAdmin.Visibility = Visibility.Visible;
                ColActions.Visibility = Visibility.Visible;
                ColPatient.Visibility = Visibility.Visible;
                PanelTopFilter.Visibility = Visibility.Visible;

                // СКРЫВАЕМ НИЖНЮЮ ПАНЕЛЬ (как просили)
                PanelFreeSlots.Visibility = Visibility.Collapsed;
                BorderDivider.Visibility = Visibility.Collapsed;

                if (_currentUser.IDRole == 1)
                    BtnCleanOld.Visibility = Visibility.Visible;
                else
                    BtnCleanOld.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadMainGrid()
        {
            try
            {
                if (_currentUser.IDRole == 3) // Пациент
                {
                    GridMain.ItemsSource = _repo.GetByPatient(_currentUser.Id);
                }
                else // Админ/Врач
                {
                    // Логика фильтрации
                    int? filterDocId = (int?)CmbFilterDoctor.SelectedValue;
                    bool onlyFree = ChkFilterFree.IsChecked == true;

                    // Получаем все записи (или по врачу)
                    var list = _repo.GetByDoctor(filterDocId);

                    // Если нужна доп. фильтрация "Только свободные" (которой нет в SQL процедуре GetDoctorAppointments)
                    // сделаем её тут, в памяти (так проще, чем менять SQL еще раз)
                    if (onlyFree)
                    {
                        list = list.Where(x => x.PatientId == null).ToList();
                    }

                    GridMain.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // --- ФИЛЬТРЫ СВЕРХУ ---
        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            LoadMainGrid();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadMainGrid();
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            CmbFilterDoctor.SelectedIndex = -1;
            ChkFilterFree.IsChecked = false;
            LoadMainGrid();
        }

        // --- УМНЫЙ ПОИСК (Имя + Телефон) ---
        private void Search_KeyUp(object sender, KeyEventArgs e)
        {
            // Работаем, если ввели что-то в имя ИЛИ в телефон
            string name = CmbSearchPatient.Text;
            string phone = TxtSearchPhone.Text;

            // Чтобы не спамить базу, ищем если хотя бы в одном поле есть 2 символа
            if (name.Length >= 2 || phone.Length >= 2)
            {
                // Вызываем поиск из ReportRepo, передавая оба параметра
                var rawList = _reportRepo.SearchPatients(name, phone);

                // Преобразуем для XAML
                CmbSearchPatient.ItemsSource = rawList.Select(p => new
                {
                    Id = p.Id,
                    DisplayText = p.DisplayText
                }).ToList();

                // Если мы печатаем в ComboBox, открываем его. Если в телефоне - не обязательно,
                // но пользователю надо видеть результат.
                CmbSearchPatient.IsDropDownOpen = true;
            }
        }

        // --- ДОБАВЛЕНИЕ / РЕДАКТИРОВАНИЕ ---
        private void BtnAddSlot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbAdminDoctor.SelectedValue == null) throw new Exception("Выберите врача");
                if (!DateTime.TryParse(TxtNewDate.Text, out DateTime date)) throw new Exception("Неверная дата");
                if (string.IsNullOrWhiteSpace(TxtNewCabinet.Text)) throw new Exception("Введите кабинет");

                int docId = (int)CmbAdminDoctor.SelectedValue;

                // --- ЛОГИКА ОПРЕДЕЛЕНИЯ ПАЦИЕНТА ---
                int? patIdToSave = null;

                // Если пользователь выбрал кого-то в поиске (SelectedValue не null) -> берем его
                if (CmbSearchPatient.SelectedValue != null)
                {
                    patIdToSave = (int)CmbSearchPatient.SelectedValue;
                }
                // Если пользователь НЕ выбирал (поиск пуст), но мы в режиме редактирования и имя пациента не меняли
                // -> берем того, кто был (_currentPatientId)
                else if (_editingSlotId != null && !string.IsNullOrWhiteSpace(CmbSearchPatient.Text))
                {
                    patIdToSave = _currentPatientId;
                }

                if (_editingSlotId == null)
                {
                    _repo.Add(docId, date, TxtNewCabinet.Text, patIdToSave);
                    MessageBox.Show("Слот добавлен!");
                }
                else
                {
                    _repo.Update(_editingSlotId.Value, docId, date, TxtNewCabinet.Text, patIdToSave);
                    MessageBox.Show("Запись обновлена!");
                    BtnCancelEdit_Click(null, null);
                }
                LoadMainGrid();
                ClearFields();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as AppointmentItem;
            if (item == null) return;

            _editingSlotId = item.Id;
            TxtNewDate.Text = item.DateVisit.ToString("dd.MM.yyyy HH:mm");
            TxtNewCabinet.Text = item.Cabinet;
            CmbAdminDoctor.SelectedValue = item.DoctorId;
            _currentPatientId = item.PatientId; // Запоминаем ID!

            if (!string.IsNullOrEmpty(item.PatientName) && item.PatientName != "—")
            {
                CmbSearchPatient.Text = item.PatientName; 
            }
            else
            {
                CmbSearchPatient.Text = "";
                _currentPatientId = null;
            }

            TxtSearchPhone.Text = "";
            BtnCancelEdit.Visibility = Visibility.Visible;
        }

        private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            _editingSlotId = null;
            BtnCancelEdit.Visibility = Visibility.Collapsed;
            ClearFields();
        }

        private void ClearFields()
        {
            TxtNewDate.Text = ""; TxtNewCabinet.Text = "";
            CmbSearchPatient.Text = ""; CmbSearchPatient.SelectedValue = null;
            TxtSearchPhone.Text = "";
            _currentPatientId = null; 
        }

        // --- УДАЛЕНИЕ ---
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as AppointmentItem;
            if (MessageBox.Show("Удалить эту запись навсегда?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _repo.Delete(item.Id);
                    LoadMainGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления: " + ex.Message);
                }
            }
        }

        // --- ОЧИСТКА ---
        private void BtnCleanOld_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить все пустые слоты, дата которых уже прошла?", "Очистка", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                int deleted = _repo.DeleteOldEmptySlots();
                MessageBox.Show($"Удалено: {deleted}");
                LoadMainGrid();
            }
        }

        private void BtnCancelDay_Click(object sender, RoutedEventArgs e)
        {
            if (CmbAdminDoctor.SelectedValue == null || DpCancelDay.SelectedDate == null) return;
            if (MessageBox.Show("Очистить расписание врача на день?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                int count = _repo.CancelDoctorDay((int)CmbAdminDoctor.SelectedValue, DpCancelDay.SelectedDate.Value);
                MessageBox.Show($"Отменено: {count}");
                LoadMainGrid();
            }
        }

        // --- НИЖНЯЯ ПАНЕЛЬ (Только для пациентов) ---
        private void CmbSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbSpec.SelectedValue is int specId) CmbDoctor.ItemsSource = _repo.GetDoctorsBySpec(specId);
        }

        private void CmbDoctor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbDoctor.SelectedValue is int docId) GridFreeSlots.ItemsSource = _repo.GetFreeSlots(doctorId: docId);
        }

        private void BtnBook_Click(object sender, RoutedEventArgs e)
        {
            var slot = (sender as Button).DataContext as AppointmentItem;
            if (MessageBox.Show($"Записаться на {slot.DisplayDate}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _repo.BookSlot(slot.Id, _currentUser.Id);
                MessageBox.Show("Успешно!");
                LoadMainGrid();
                if (CmbDoctor.SelectedValue is int docId) GridFreeSlots.ItemsSource = _repo.GetFreeSlots(doctorId: docId);
            }
        }
    }
}