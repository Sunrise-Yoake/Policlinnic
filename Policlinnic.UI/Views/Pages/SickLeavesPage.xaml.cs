using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

            // Загружаем данные и сразу применяем сортировку, 
            // чтобы стрелочка отобразилась корректно (зеленая вниз)
            LoadData();

            this.Loaded += SickLeavesPage_Loaded;
            _isLoaded = true;
        }

        private void SickLeavesPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (PanelSpecFilter.Visibility == Visibility.Visible)
                CmbSpec.Focus();
            else
                CmbStatus.Focus();
        }

        private void SetupRoleAccess()
        {
            if (_currentUser.IDRole == 3) // Пациент
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                ColActions.Visibility = Visibility.Collapsed;
                PanelSpecFilter.Visibility = Visibility.Collapsed;
                // Врач виден пациенту
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

                ApplyFilters();

                // МАГИЯ СОРТИРОВКИ:
                // Это заставит DataGrid обновить стрелочки в нашем кастомном заголовке
                var view = CollectionViewSource.GetDefaultView(GridSickLeaves.ItemsSource);
                if (view != null)
                {
                    view.SortDescriptions.Clear();
                    // Сначала открытые, потом новые
                    view.SortDescriptions.Add(new SortDescription("IsOpen", ListSortDirection.Descending));
                    view.SortDescriptions.Add(new SortDescription("RawDateStart", ListSortDirection.Descending));
                    view.Refresh();

                    // Насильно говорим колонке, что она отсортирована, чтобы триггер в XAML сработал
                    foreach (var col in GridSickLeaves.Columns)
                    {
                        if (col.SortMemberPath == "RawDateStart") // Колонка "Начало"
                        {
                            col.SortDirection = ListSortDirection.Descending;
                            break;
                        }
                    }
                }
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

            if (PanelSpecFilter.Visibility == Visibility.Visible && CmbSpec.SelectedIndex > 0)
            {
                string selectedSpec = CmbSpec.SelectedItem.ToString();
                query = query.Where(x => x.SpecName == selectedSpec);
            }

            if (CmbStatus.SelectedIndex == 1)
                query = query.Where(x => x.IsOpen);
            else if (CmbStatus.SelectedIndex == 2)
                query = query.Where(x => !x.IsOpen);

            GridSickLeaves.ItemsSource = query.ToList();
        }

        private void Filters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded)
            {
                ApplyFilters();
                // Важно: чтобы при фильтрации не слетала сортировка
                var view = CollectionViewSource.GetDefaultView(GridSickLeaves.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("IsOpen", ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription("RawDateStart", ListSortDirection.Descending));
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Добавить"); }
        private void BtnEdit_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Редактировать"); }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as SickLeaveView;
            if (item != null && MessageBox.Show("Удалить?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _allData.Remove(item);
                Filters_SelectionChanged(null, null);
            }
        }
    }
}