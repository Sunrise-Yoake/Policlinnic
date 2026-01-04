using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Policlinnic.BLL.Services; // Подключаем Service
using Policlinnic.Domain.Entities;
using Policlinnic.UI.Views; // Это позволит видеть SickLeaveWindow
namespace Policlinnic.UI.Views.Pages
{
    public partial class SickLeavesPage : Page
    {
        private readonly SickLeaveService _service = new SickLeaveService();
        private readonly User _currentUser;

        private List<SickLeaveView> _allData = new List<SickLeaveView>();
        private bool _isLoaded = false;

        public SickLeavesPage(User user)
        {
            InitializeComponent();
            _currentUser = user;

            SetupRoleAccess();
            LoadFilters();
            LoadData();

            this.Loaded += (s, e) =>
            {
                _isLoaded = true;
                // Фокус на нужный фильтр при загрузке
                if (PanelSpecFilter.Visibility == Visibility.Visible) CmbSpec.Focus();
                else CmbStatus.Focus();
            };
        }

        private void SetupRoleAccess()
        {
            if (_currentUser.IDRole == 3) // Пациент
            {
                BtnAdd.Visibility = Visibility.Collapsed;
                ColActions.Visibility = Visibility.Collapsed;
                PanelSpecFilter.Visibility = Visibility.Collapsed;
            }
            else // Врач/Админ
            {
                BtnAdd.Visibility = Visibility.Visible;
                ColActions.Visibility = Visibility.Visible;
                ColDoctor.Visibility = Visibility.Visible;
                PanelSpecFilter.Visibility = Visibility.Visible;
            }
        }

        private void LoadFilters()
        {
            CmbSpec.Items.Clear();
            CmbSpec.Items.Add("Все специализации");

            var specs = _service.GetSpecializations();
            foreach (var s in specs) CmbSpec.Items.Add(s);

            CmbSpec.SelectedIndex = 0;
        }

        private void LoadData()
        {
            try
            {
                // ВСЯ логика загрузки теперь в одну строчку через BLL
                _allData = _service.GetList(_currentUser);

                ApplyFilters();
                ApplySorting(); // Вынесли сортировку в отдельный метод
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySorting()
        {
            // Магия для стрелочек в заголовках
            var view = CollectionViewSource.GetDefaultView(GridSickLeaves.ItemsSource);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("IsOpen", ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription("RawDateStart", ListSortDirection.Descending));
                view.Refresh();

                // Визуально ставим стрелку на колонке
                foreach (var col in GridSickLeaves.Columns)
                {
                    if (col.SortMemberPath == "RawDateStart")
                    {
                        col.SortDirection = ListSortDirection.Descending;
                        break;
                    }
                }
            }
        }

        private void ApplyFilters()
        {
            if (_allData == null) return;
            var query = _allData.AsEnumerable();

            // 1. Фильтр по специализации
            if (PanelSpecFilter.Visibility == Visibility.Visible && CmbSpec.SelectedIndex > 0)
            {
                string selectedSpec = CmbSpec.SelectedItem.ToString();
                query = query.Where(x => x.SpecName == selectedSpec);
            }

            // 2. Фильтр по статусу
            if (CmbStatus.SelectedIndex == 1) // Незакрытые
                query = query.Where(x => x.IsOpen);
            else if (CmbStatus.SelectedIndex == 2) // Закрытые
                query = query.Where(x => !x.IsOpen);

            GridSickLeaves.ItemsSource = query.ToList();
        }

        private void Filters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded)
            {
                ApplyFilters();
                ApplySorting(); // Чтобы при фильтрации не слетала сортировка
            }
        }

        // --- КНОПКИ ---
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Теперь SickLeaveWindow доступен напрямую в пространстве Views
            var win = new SickLeaveWindow();
            if (win.ShowDialog() == true)
            {
                LoadData(); // Метод из прошлого шага для обновления списка
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedView = (sender as Button)?.DataContext as SickLeaveView;
            if (selectedView == null) return;

            // Конвертируем View-модель обратно в Entity для передачи в окно
            var entity = new SickLeave
            {
                Id = selectedView.Id,
                IDPatient = selectedView.IDPatient,
                IDDoctor = selectedView.IDDoctor,
                DateStart = selectedView.RawDateStart,
                DateEnd = selectedView.IsOpen ? null : (DateTime?)DateTime.Parse(selectedView.DateEnd)
            };

            var win = new SickLeaveWindow(entity);
            if (win.ShowDialog() == true)
            {
                LoadData();
            }
        }
        // Внутри класса SickLeavesPage (в файле Views/Pages/SickLeavesPage.xaml.cs)

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Получаем объект из строки, где нажата кнопка
            var item = (sender as Button)?.DataContext as SickLeaveView;
            if (item == null) return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить больничный №{item.Id}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Вызываем удаление через сервис
                    _service.Delete(item.Id, _currentUser);

                    // Обновляем список данных на странице
                    LoadData();
                    MessageBox.Show("Запись успешно удалена.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}