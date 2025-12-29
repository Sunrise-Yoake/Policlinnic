using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;
using Excel = Microsoft.Office.Interop.Excel;

namespace Policlinnic.UI.Views.Pages
{
    public partial class ReportsPage : Page
    {
        private readonly ReportRepository _repository;

        public ReportsPage()
        {
            InitializeComponent();
            _repository = new ReportRepository();
            CmbReportType_SelectionChanged(null, null);
        }

        private void CmbReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelPatientSearch == null) return;

            if (CmbReportType.SelectedIndex == 0) // Финансы
            {
                PanelPatientSearch.Visibility = Visibility.Collapsed;
                GridPaidServices.Visibility = Visibility.Visible;

                // ПОКАЗЫВАЕМ ИТОГО
                FooterTotal.Visibility = Visibility.Visible;
                ContainerPatientHistory.Visibility = Visibility.Collapsed;

                LoadFinancialReport();
            }
            else // История пациента
            {
                PanelPatientSearch.Visibility = Visibility.Visible;
                GridPaidServices.Visibility = Visibility.Collapsed;

                // СКРЫВАЕМ ИТОГО (там оно не нужно)
                FooterTotal.Visibility = Visibility.Collapsed;
                ContainerPatientHistory.Visibility = Visibility.Visible;
                GridPatientHistory.ItemsSource = null;

                TxtNameSearch.Text = "";
                TxtPhoneSearch.Text = "";
            }
        }

        private void LoadFinancialReport()
        {
            try
            {
                // 1. Грузим таблицу
                var data = _repository.GetPaidServicesReport();
                GridPaidServices.ItemsSource = data;

                // 2. Грузим ИТОГО через скалярную функцию
                decimal total = _repository.GetTotalIncome();

                // Форматируем как валюту (C - Currency)
                TxtTotalIncome.Text = total.ToString("C");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }


        private void TxtPatientSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string namePart = TxtNameSearch.Text.Trim();
            string phonePart = TxtPhoneSearch.Text.Trim();

            if (namePart.Length > 0 || phonePart.Length > 0)
            {
                var candidates = _repository.SearchPatients(namePart, phonePart);

                if (candidates.Count > 0)
                {
                    LstPatientSuggestions.ItemsSource = candidates;
                    LstPatientSuggestions.Visibility = Visibility.Visible;
                }
                else
                    LstPatientSuggestions.Visibility = Visibility.Collapsed;
            }
            else
                LstPatientSuggestions.Visibility = Visibility.Collapsed;
        }

        private void LstPatientSuggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstPatientSuggestions.SelectedItem is PatientLookupItem selectedPatient)
            {
                var parts = selectedPatient.DisplayText.Split('|');
                string name = parts[0].Trim();
                string phone = parts.Length > 1 ? parts[1].Trim() : "";

                TxtNameSearch.TextChanged -= TxtPatientSearch_TextChanged;
                TxtPhoneSearch.TextChanged -= TxtPatientSearch_TextChanged;

                TxtNameSearch.Text = name;
                TxtPhoneSearch.Text = phone;

                TxtNameSearch.TextChanged += TxtPatientSearch_TextChanged;
                TxtPhoneSearch.TextChanged += TxtPatientSearch_TextChanged;

                LstPatientSuggestions.Visibility = Visibility.Collapsed;

                LoadPatientHistory(selectedPatient.Id);
            }
        }

        //  Очистка поиска
        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtNameSearch.Text = "";
            TxtPhoneSearch.Text = "";
            LstPatientSuggestions.Visibility = Visibility.Collapsed;

            TxtNameSearch.Focus();
        }

        private void LoadPatientHistory(int patientId)
        {
            try
            {
                var data = _repository.GetPatientHistory(patientId);
                GridPatientHistory.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки истории: " + ex.Message);
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGrid activeGrid = null;
                string fileName = "";
                string reportTitleInExcel = "";

                if (GridPaidServices.Visibility == Visibility.Visible)
                {
                    activeGrid = GridPaidServices;
                    fileName = "Отчет_ПлатныеУслуги";
                }
                else
                {
                    activeGrid = GridPatientHistory;
                    string patientName = TxtNameSearch.Text.Replace(" ", "_");
                    if (string.IsNullOrEmpty(patientName)) patientName = "Пациент";

                    fileName = $"История_Болезни_{patientName}";
                    reportTitleInExcel = $"ОТЧЁТ ПО ПАЦИЕНТУ: {TxtNameSearch.Text}";
                }

                if (activeGrid != null && activeGrid.HasItems)
                {
                    ExportToExcel(activeGrid, fileName, reportTitleInExcel);
                }
                else
                {
                    MessageBox.Show("Нет данных для экспорта.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта: " + ex.Message);
            }
        }

        private void ExportToExcel(DataGrid grid, string fileNamePrefix, string reportTitle = "")
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel файл (*.xlsx)|*.xlsx";
            saveDialog.FileName = $"{fileNamePrefix}_{DateTime.Now:yyyy-MM-dd}";

            if (saveDialog.ShowDialog() == true)
            {
                var excelApp = new Excel.Application();
                excelApp.Visible = false;
                var workbook = excelApp.Workbooks.Add();
                var sheet = (Excel.Worksheet)excelApp.ActiveSheet;

                try
                {
                    int startRow = string.IsNullOrEmpty(reportTitle) ? 1 : 3;

                    // 1. ЗАГОЛОВОК (Если есть - для пациента)
                    if (!string.IsNullOrEmpty(reportTitle))
                    {
                        sheet.Cells[1, 1] = reportTitle;
                        Excel.Range titleRange = sheet.Range[sheet.Cells[1, 1], sheet.Cells[1, grid.Columns.Count]];
                        titleRange.Merge();
                        titleRange.Font.Bold = true;
                        titleRange.Font.Size = 14;
                        titleRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(46, 125, 50));
                    }

                    // 2. ШАПКА ТАБЛИЦЫ
                    var columns = grid.Columns;
                    for (int i = 0; i < columns.Count; i++)
                    {
                        sheet.Cells[startRow, i + 1] = columns[i].Header.ToString();
                    }

                    // 3. ДАННЫЕ
                    var items = grid.ItemsSource as System.Collections.IEnumerable;
                    int row = startRow + 1;
                    decimal financialTotal = 0;

                    foreach (var item in items)
                    {
                        if (item is PaidServiceReportItem p)
                        {
                            sheet.Cells[row, 1] = p.DoctorName;
                            sheet.Cells[row, 2] = p.Spec;
                            sheet.Cells[row, 3] = p.ServiceName;
                            sheet.Cells[row, 4] = p.CountServices;
                            // Пишем как число (double), чтобы Excel сам понял формат
                            sheet.Cells[row, 5] = (double)p.TotalIncome;

                            financialTotal += p.TotalIncome;
                        }
                        else if (item is PatientHistoryItem h)
                        {
                            sheet.Cells[row, 1] = h.DateVisit.ToString("dd.MM.yyyy");
                            sheet.Cells[row, 2] = h.DoctorName;
                            sheet.Cells[row, 3] = h.Spec;
                            sheet.Cells[row, 4] = h.Diagnosis;
                            sheet.Cells[row, 5] = h.Medicine;
                            sheet.Cells[row, 6] = h.TreatmentInfo;
                        }
                        row++;
                    }

                    // 4. ИТОГО (Только для финансового отчета)
                    if (grid == GridPaidServices)
                    {
                        // Просто пишем текст и число. Никакого NumberFormat.
                        sheet.Cells[row, 4] = "ИТОГО:";
                        sheet.Cells[row, 5] = (double)financialTotal;

                        // Красим строку
                        Excel.Range totalRange = sheet.Range[sheet.Cells[row, 1], sheet.Cells[row, 5]];
                        totalRange.Font.Bold = true;
                        totalRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(241, 248, 233));

                        // Включаем эту строку в границы
                        row++;
                    }

                    // 5. ОФОРМЛЕНИЕ (РАМКИ)
                    Excel.Range headerRange = sheet.Range[sheet.Cells[startRow, 1], sheet.Cells[startRow, columns.Count]];
                    headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(200, 230, 201));
                    headerRange.Font.Bold = true;
                    headerRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                    Excel.Range fullRange = sheet.Range[sheet.Cells[startRow, 1], sheet.Cells[row - 1, columns.Count]];
                    fullRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                    sheet.Columns.AutoFit();
                    workbook.SaveAs(saveDialog.FileName);

                    if (MessageBox.Show("Отчет сохранен. Открыть?", "Excel", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        excelApp.Visible = true;
                    else
                    {
                        workbook.Close(false);
                        excelApp.Quit();
                    }
                }
                catch
                {
                    workbook.Close(false);
                    excelApp.Quit();
                    throw;
                }
            }
        }
    }
}