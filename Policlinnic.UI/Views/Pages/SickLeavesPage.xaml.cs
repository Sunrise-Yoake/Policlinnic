using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;

namespace Policlinnic.UI.Views.Pages
{
    public partial class SickLeavesPage : Page
    {
        private readonly SickLeaveRepository _repository;
        private readonly User _currentUser;
        private List<SickLeaveView>? _allData = new List<SickLeaveView>();
        private bool _isLoaded = false;

        public SickLeavesPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _repository = new SickLeaveRepository();

            SetupRoleAccess();
            LoadFilters();
            LoadData();
            _isLoaded = true;
        }

        private void SetupRoleAccess()
        {
            if (_currentUser.IDRole == 3) // Пациент
            {
                // Скрываем кнопки редактирования
                BtnAdd.Visibility = Visibility.Collapsed;
                ColActions.Visibility = Visibility.Collapsed;

                // Скрываем колонку "Врач" - пациенту не нужно видеть, кто врач (он и так знает или это есть в деталях)
                // Или если ты хочешь скрыть, потому что "ненужная колонка по врачам"
                ColDoctor.Visibility = Visibility.Collapsed;

                // Скрываем фильтр специализации
                PanelSpecFilter.Visibility = Visibility.Collapsed;
            }
            else
            {
                BtnAdd.Visibility = Visibility.Visible;
                ColActions.Visibility = Visibility.Visible;
                ColDoctor.Visibility = Visibility.Visible;
                PanelSpecFilter.Visibility = Visibility.Visible;
            }
        }

        private void LoadFilters()
        {
            try
            {
                var specs = _repository.GetSpecializationNames();
                CmbSpec.Items.Add("Все специализации");
                foreach (var s in specs) CmbSpec.Items.Add(s);
                CmbSpec.SelectedIndex = 0;
            }
            catch { }
        }

        private void LoadData()
        {
            try
            {
                if (_currentUser.IDRole == 3)
                    _allData = _repository.GetByIDPatient(_currentUser.Id);
                else
                    _allData = _repository.GetAllSickLeaves();

                // Первичная сортировка при загрузке (чтобы было красиво сразу)
                // Новые (незакрытые) сверху, потом по дате
                _allData = _allData
                    .OrderByDescending(x => x.IsOpen)
                    .ThenByDescending(x => x.RawDateStart)
                    .ToList();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void ApplyFilters()
        {
            if (_allData == null) return;
            var query = _allData.AsEnumerable();

            // 1. Фильтр Специализации (только если виден)
            if (PanelSpecFilter.Visibility == Visibility.Visible && CmbSpec.SelectedIndex > 0)
            {
                string selectedSpec = CmbSpec.SelectedItem.ToString();
                query = query.Where(x => x.SpecName == selectedSpec);

                PanelDoctorFilter.Visibility = Visibility.Visible;
                UpdateDoctorFilter(query.ToList());
            }
            else
            {
                PanelDoctorFilter.Visibility = Visibility.Collapsed;
                CmbDoctor.SelectedIndex = -1;
            }

            // 2. Фильтр Врача
            if (CmbDoctor.SelectedIndex > 0 && PanelDoctorFilter.Visibility == Visibility.Visible)
            {
                string selectedDoc = CmbDoctor.SelectedItem.ToString();
                query = query.Where(x => x.DoctorFIO == selectedDoc);
            }

            // 3. Статус
            if (CmbStatus.SelectedIndex == 1) // Незакрытые
                query = query.Where(x => x.IsOpen);
            else if (CmbStatus.SelectedIndex == 2) // Закрытые
                query = query.Where(x => !x.IsOpen);

            // 4. Сортировка - МЫ ЕЕ УБРАЛИ из кода
            // DataGrid сам справится, так как CanUserSortColumns="True"

            GridSickLeaves.ItemsSource = query.ToList();
        }

        private void UpdateDoctorFilter(List<SickLeaveView> filteredBySpec)
        {
            string? currentSelection = CmbDoctor.SelectedItem as string;

            var doctors = filteredBySpec
                .Select(x => x.DoctorFIO)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            CmbDoctor.SelectionChanged -= Filters_SelectionChanged;

            CmbDoctor.Items.Clear();
            CmbDoctor.Items.Add("Все врачи");
            foreach (var doc in doctors) CmbDoctor.Items.Add(doc);

            if (currentSelection != null && CmbDoctor.Items.Contains(currentSelection))
                CmbDoctor.SelectedItem = currentSelection;
            else
                CmbDoctor.SelectedIndex = 0;

            CmbDoctor.SelectionChanged += Filters_SelectionChanged;
        }

        private void Filters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded) ApplyFilters();
        }

        // Кнопки действий
        private void BtnAdd_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Добавление"); }
        private void BtnEdit_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Редактирование"); }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as SickLeaveView;
            if (item != null && MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _allData.Remove(item);
                ApplyFilters();
            }
        }
    }
}