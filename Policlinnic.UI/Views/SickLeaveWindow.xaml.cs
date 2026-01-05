using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Policlinnic.BLL.Services;
using Policlinnic.DAL.Repositories;
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

        // ВЫБОР ПАЦИЕНТА
        private void LstPatientResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstPatientResults.SelectedItem is PatientLookupItem item)
            {
                _selectedPatientId = item.Id;
                var parts = item.DisplayText.Split(new[] { " | " }, StringSplitOptions.None);

                TxtPatientName.TextChanged -= PatientSearch_Changed;
                TxtPatientPhone.TextChanged -= PatientSearch_Changed;

                TxtPatientName.Text = parts[0].Trim();
                if (parts.Length > 1) TxtPatientPhone.Text = parts[1].Trim();

                TxtPatientName.TextChanged += PatientSearch_Changed;
                TxtPatientPhone.TextChanged += PatientSearch_Changed;
                LstPatientResults.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearPatient_Click(object sender, RoutedEventArgs e)
        {
            _selectedPatientId = null;
            TxtPatientName.Clear();
            TxtPatientPhone.Clear();
            LstPatientResults.Visibility = Visibility.Collapsed;
        }

        // ВЫБОР ВРАЧА
        private void LstDoctorResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstDoctorResults.SelectedItem is DoctorLookupItem item)
            {
                _selectedDoctorId = item.Id;
                var parts = item.DisplayText.Split(new[] { " | " }, StringSplitOptions.None);

                TxtDoctorName.TextChanged -= DoctorSearch_Changed;
                TxtDoctorSpec.TextChanged -= DoctorSearch_Changed;

                TxtDoctorName.Text = parts[0].Trim();
                if (parts.Length > 1) TxtDoctorSpec.Text = parts[1].Trim();

                TxtDoctorName.TextChanged += DoctorSearch_Changed;
                TxtDoctorSpec.TextChanged += DoctorSearch_Changed;
                LstDoctorResults.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearDoctor_Click(object sender, RoutedEventArgs e)
        {
            _selectedDoctorId = null;
            TxtDoctorName.Clear();
            TxtDoctorSpec.Clear();
            LstDoctorResults.Visibility = Visibility.Collapsed;
        }

        private void PatientSearch_Changed(object sender, TextChangedEventArgs e)
        {
            if (_lookupRepository == null) return;
            var results = _lookupRepository.SearchPatientsLookup(TxtPatientName.Text, TxtPatientPhone.Text);
            LstPatientResults.ItemsSource = results;
            LstPatientResults.Visibility = results.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DoctorSearch_Changed(object sender, TextChangedEventArgs e)
        {
            if (_lookupRepository == null) return;
            var results = _lookupRepository.SearchDoctorsLookup(TxtDoctorName.Text, TxtDoctorSpec.Text);
            LstDoctorResults.ItemsSource = results;
            LstDoctorResults.Visibility = results.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPatientId == null || _selectedDoctorId == null || DpStart.SelectedDate == null)
            {
                MessageBox.Show("Сначала выберите Пациента и Врача из списка!");
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

            try { _service.Save(Result); DialogResult = true; }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void ChkIsOpen_Checked(object sender, RoutedEventArgs e)
        {
            if (DpEnd != null) { DpEnd.IsEnabled = false; DpEnd.SelectedDate = null; }
        }

        private void ChkIsOpen_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DpEnd != null) { DpEnd.IsEnabled = true; if (DpEnd.SelectedDate == null) DpEnd.SelectedDate = DateTime.Now; }
        }
    }
}