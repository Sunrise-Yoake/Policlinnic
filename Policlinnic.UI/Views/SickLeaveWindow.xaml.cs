using System;
using System.Windows;
using Policlinnic.BLL.Services;
using Policlinnic.Domain.Entities;

namespace Policlinnic.UI.Views
{
    public partial class SickLeaveWindow : Window
    {
        private readonly SickLeaveService _service = new SickLeaveService();
        public SickLeave Result { get; private set; } // Объект для возврата

        // Конструктор: если sickLeave == null, значит режим добавления
        public SickLeaveWindow(SickLeave sickLeave = null)
        {
            InitializeComponent();
            LoadCombos();

            if (sickLeave != null)
            {
                // Режим редактирования: заполняем поля
                Result = sickLeave;
                CmbPatient.SelectedValue = sickLeave.IDPatient;
                CmbDoctor.SelectedValue = sickLeave.IDDoctor;
                DpStart.SelectedDate = sickLeave.DateStart;

                if (sickLeave.DateEnd.HasValue)
                {
                    DpEnd.SelectedDate = sickLeave.DateEnd;
                    ChkIsOpen.IsChecked = false;
                }
                else
                {
                    ChkIsOpen.IsChecked = true;
                }
            }
            else
            {
                // Режим добавления
                Result = new SickLeave();
                DpStart.SelectedDate = DateTime.Now;
                ChkIsOpen.IsChecked = true; // По умолчанию открыт
            }
        }

        private void LoadCombos()
        {
            try
            {
                CmbPatient.ItemsSource = _service.GetPatients();
                CmbDoctor.ItemsSource = _service.GetDoctors();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списков: " + ex.Message);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CmbPatient.SelectedValue == null || CmbDoctor.SelectedValue == null || DpStart.SelectedDate == null)
            {
                MessageBox.Show("Заполните обязательные поля (Пациент, Врач, Дата начала)!");
                return;
            }

            // Заполняем объект данными из формы
            Result.IDPatient = (int)CmbPatient.SelectedValue;
            Result.IDDoctor = (int)CmbDoctor.SelectedValue;
            Result.DateStart = DpStart.SelectedDate.Value;

            if (ChkIsOpen.IsChecked == true)
                Result.DateEnd = null;
            else
                Result.DateEnd = DpEnd.SelectedDate;

            // Валидация
            if (Result.DateEnd.HasValue && Result.DateEnd < Result.DateStart)
            {
                MessageBox.Show("Дата окончания не может быть раньше начала!");
                return;
            }

            // Пытаемся сохранить через сервис
            try
            {
                _service.Save(Result);
                DialogResult = true; // Закрывает окно и возвращает true
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ChkIsOpen_Checked(object sender, RoutedEventArgs e)
        {
            DpEnd.IsEnabled = false;
            DpEnd.SelectedDate = null;
        }

        private void ChkIsOpen_Unchecked(object sender, RoutedEventArgs e)
        {
            DpEnd.IsEnabled = true;
            if (DpEnd.SelectedDate == null) DpEnd.SelectedDate = DateTime.Now;
        }
    }
}