using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Policlinnic.BLL.Services;
using Policlinnic.DAL.Repositories; // Путь к вашему новому репозиторию
using Policlinnic.Domain.Entities;

namespace Policlinnic.UI.Views
{
    public partial class SickLeaveWindow : Window
    {
        private readonly SickLeaveService _service = new SickLeaveService();
        private readonly LookupRepository _lookupRepository = new LookupRepository();

        public SickLeave Result { get; private set; }

        private int? _selectedPatientId;
        private int? _selectedDoctorId;

        public SickLeaveWindow(SickLeave sickLeave = null)
        {
            InitializeComponent();

            if (sickLeave != null)
            {
                Result = sickLeave;
                _selectedPatientId = sickLeave.IDPatient;
                _selectedDoctorId = sickLeave.IDDoctor;

                // В режиме редактирования отображаем ID (или можно подтянуть имена через доп. метод)
                TxtPatientName.Text = $"ID: {sickLeave.IDPatient}";
                TxtDoctorName.Text = $"ID: {sickLeave.IDDoctor}";

                DpStart.SelectedDate = sickLeave.DateStart;
                if (sickLeave.DateEnd.HasValue)
                {
                    DpEnd.SelectedDate = sickLeave.DateEnd;
                    ChkIsOpen.IsChecked = false;
                }
                else ChkIsOpen.IsChecked = true;
            }
            else
            {
                Result = new SickLeave();
                DpStart.SelectedDate = DateTime.Now;
                ChkIsOpen.IsChecked = true;
            }
        }

        // ПОИСК ПАЦИЕНТА
        private void PatientSearch_Changed(object sender, TextChangedEventArgs e)
        {
            string searchText = TxtPatientSearch.Text;
            BtnClearPatient.Visibility = string.IsNullOrEmpty(searchText) ? Visibility.Collapsed : Visibility.Visible;

            if (_lookupRepository == null) return;

            // Передаем один и тот же текст в оба параметра поиска (имя и телефон)
            // чтобы БД искала совпадение в любом из этих полей
            var results = _lookupRepository.SearchPatientsLookup(searchText, searchText);
            LstPatientResults.ItemsSource = results;
            LstPatientResults.Visibility = results.Any() ? Visibility.Visible : Visibility.Collapsed;
        }
        // ОЧИСТКА ПОЛЯ ПАЦИЕНТА
        private void BtnClearPatient_Click(object sender, RoutedEventArgs e)
        {
            TxtPatientSearch.Clear();
            _selectedPatientId = null;
            LstPatientResults.Visibility = Visibility.Collapsed;
        }

        private void LstPatientResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstPatientResults.SelectedItem is PatientLookupItem item)
            {
                _selectedPatientId = item.Id;
                TxtPatientName.Text = item.DisplayText; // Показываем выбранное
                LstPatientResults.Visibility = Visibility.Collapsed;
            }
        }

        // ПОИСК ВРАЧА
        private void DoctorSearch_Changed(object sender, TextChangedEventArgs e)
        {
            string searchText = TxtDoctorSearch.Text;
            BtnClearDoctor.Visibility = string.IsNullOrEmpty(searchText) ? Visibility.Collapsed : Visibility.Visible;

            if (_lookupRepository == null) return;

            var results = _lookupRepository.SearchDoctorsLookup(searchText, searchText);
            LstDoctorResults.ItemsSource = results;
            LstDoctorResults.Visibility = results.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        // ОЧИСТКА ПОЛЯ ВРАЧА
        private void BtnClearDoctor_Click(object sender, RoutedEventArgs e)
        {
            TxtDoctorSearch.Clear();
            _selectedDoctorId = null;
            LstDoctorResults.Visibility = Visibility.Collapsed;
        }

        private void LstDoctorResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstDoctorResults.SelectedItem is DoctorLookupItem item)
            {
                _selectedDoctorId = item.Id;
                TxtDoctorName.Text = item.DisplayText;
                LstDoctorResults.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPatientId == null || _selectedDoctorId == null || DpStart.SelectedDate == null)
            {
                MessageBox.Show("Сначала выберите Пациента и Врача из предложенного списка!");
                return;
            }

            Result.IDPatient = _selectedPatientId.Value;
            Result.IDDoctor = _selectedDoctorId.Value;
            Result.DateStart = DpStart.SelectedDate.Value;
            Result.DateEnd = ChkIsOpen.IsChecked == true ? null : DpEnd.SelectedDate;

            if (Result.DateEnd.HasValue && Result.DateEnd < Result.DateStart)
            {
                MessageBox.Show("Ошибка: Дата окончания раньше начала!");
                return;
            }

            try
            {
                _service.Save(Result);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Остальные методы (Cancel, CheckBox) остаются без изменений...
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void ChkIsOpen_Checked(object sender, RoutedEventArgs e)
        {
            if (DpEnd == null) return;
            DpEnd.IsEnabled = false;
            DpEnd.SelectedDate = null;
        }

        private void ChkIsOpen_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DpEnd == null) return;
            DpEnd.IsEnabled = true;
            if (DpEnd.SelectedDate == null) DpEnd.SelectedDate = DateTime.Now;
        }
    }
}