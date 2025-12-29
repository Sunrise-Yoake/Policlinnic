using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32; // Для диалога сохранения
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices; // Для очистки COM-объектов

namespace Policlinnic.UI.Views.Pages
{
    public partial class DictionariesPage : Page
    {
        private readonly DictionaryRepository _repository;
        private string _currentDictionary = "Лекарства";

        public DictionariesPage()
        {
            InitializeComponent();
            _repository = new DictionaryRepository();
            LoadData();
        }

        private void CmbDictionaryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridMedicines == null || GridIllnesses == null || GridSpecs == null) return;

            if (CmbDictionaryType.SelectedItem is ComboBoxItem item)
            {
                _currentDictionary = item.Content.ToString();
                UpdateVisibility();
                LoadData();
            }
        }

        private void UpdateVisibility()
        {
            GridMedicines.Visibility = Visibility.Collapsed;
            GridIllnesses.Visibility = Visibility.Collapsed;
            GridSpecs.Visibility = Visibility.Collapsed;

            switch (_currentDictionary)
            {
                case "Лекарства": GridMedicines.Visibility = Visibility.Visible; break;
                case "Болезни": GridIllnesses.Visibility = Visibility.Visible; break;
                case "Специализации": GridSpecs.Visibility = Visibility.Visible; break;
            }
        }

        private void LoadData()
        {
            try
            {
                switch (_currentDictionary)
                {
                    case "Лекарства": GridMedicines.ItemsSource = _repository.GetAllMedicines(); break;
                    case "Болезни": GridIllnesses.ItemsSource = _repository.GetAllIllnesses(); break;
                    case "Специализации": GridSpecs.ItemsSource = _repository.GetAllSpecializations(); break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        // --- КНОПКИ ---
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new DictionaryEditWindow(_currentDictionary, null);
            window.ShowDialog();

            if (window.IsSuccess)
            {
                LoadData();
            }
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Получаем объект из строки, где нажали кнопку
            var item = ((FrameworkElement)sender).DataContext;

            if (item != null)
            {
                var window = new DictionaryEditWindow(_currentDictionary, item);
                window.ShowDialog();

                if (window.IsSuccess)
                {
                    LoadData(); // Обновляем таблицу
                }
            }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext;
            if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    int id = 0; string table = "";
                    if (item is Medicine m) { id = m.ID; table = "Лекарство"; }
                    else if (item is Illness i) { id = i.ID; table = "Болезнь"; }
                    else if (item is Specialization s) { id = s.ID; table = "Специализация"; }

                    if (id > 0) { _repository.DeleteEntity(table, id); LoadData(); }
                }
                catch (Exception ex) { MessageBox.Show("Ошибка удаления: " + ex.Message); }
            }
        }

        // --- ЭКСПОРТ В EXCEL ---
        private void BtnReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGrid grid = null;
                if (GridMedicines.Visibility == Visibility.Visible) grid = GridMedicines;
                else if (GridIllnesses.Visibility == Visibility.Visible) grid = GridIllnesses;
                else if (GridSpecs.Visibility == Visibility.Visible) grid = GridSpecs;

                if (grid != null && grid.ItemsSource != null)
                {
                    ExportWithSaveDialog(grid, _currentDictionary);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта:\n" + ex.Message);
            }
        }

        private void ExportWithSaveDialog(DataGrid grid, string title)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel файл (*.xlsx)|*.xlsx";
            saveDialog.FileName = $"Отчет_{title}_{DateTime.Now:yyyy-MM-dd}";

            if (saveDialog.ShowDialog() == true)
            {
                string path = saveDialog.FileName;

                Excel.Application excelApp = null;
                Excel.Workbook workbook = null;
                Excel._Worksheet sheet = null;

                try
                {
                    excelApp = new Excel.Application();
                    excelApp.Visible = false; // Скрываем Excel во время заполнения

                    workbook = excelApp.Workbooks.Add();
                    sheet = (Excel.Worksheet)excelApp.ActiveSheet;

                    // Заголовки
                    var columns = grid.Columns.Where(c => c.Header.ToString() != "Действия").ToList();
                    for (int i = 0; i < columns.Count; i++)
                    {
                        sheet.Cells[1, i + 1] = columns[i].Header.ToString();
                    }

                    // Данные
                    var items = grid.ItemsSource.Cast<object>().ToList();
                    int row = 2;
                    foreach (var item in items)
                    {
                        if (item is Medicine m) { sheet.Cells[row, 1] = m.Name; sheet.Cells[row, 2] = m.FoodDependency; }
                        else if (item is Illness i) { sheet.Cells[row, 1] = i.Name; sheet.Cells[row, 2] = i.Notes; }
                        else if (item is Specialization s) { sheet.Cells[row, 1] = s.Name; }
                        row++;
                    }

                    // Дизайн (Зеленая шапка + Рамки)
                    Excel.Range headerRange = sheet.Range[sheet.Cells[1, 1], sheet.Cells[1, columns.Count]];
                    headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(200, 230, 201));
                    headerRange.Font.Bold = true;
                    headerRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                    Excel.Range fullRange = sheet.Range[sheet.Cells[1, 1], sheet.Cells[row - 1, columns.Count]];
                    fullRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                    sheet.Columns.AutoFit();

                    // Сохраняем
                    workbook.SaveAs(path);

                    // СПРАШИВАЕМ ПОЛЬЗОВАТЕЛЯ
                    var result = MessageBox.Show($"Отчет успешно сохранен по пути:\n{path}\n\nОткрыть файл сейчас?",
                                                 "Готово",
                                                 MessageBoxButton.YesNo,
                                                 MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        excelApp.Visible = true; // Показываем Excel
                    }
                    else
                    {
                        // Закрываем и чистим память, если пользователь отказался
                        workbook.Close(false);
                        excelApp.Quit();
                        ReleaseObject(sheet);
                        ReleaseObject(workbook);
                        ReleaseObject(excelApp);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при создании Excel: " + ex.Message);
                    // Аварийное закрытие
                    if (excelApp != null) excelApp.Quit();
                }
            }
        }

        private void ReleaseObject(object obj)
        {
            try
            {
                if (obj != null) Marshal.ReleaseComObject(obj);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка очистки объекта: " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}