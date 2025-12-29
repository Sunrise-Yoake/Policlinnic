using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using Policlinnic.DAL.Repositories;

namespace Policlinnic.UI.Views.Pages
{
    public partial class StatisticsPage : Page
    {
        private readonly StatsRepository _repository;

        // Свойство для меток оси X
        public List<string> DateLabels { get; set; }

        // ИСПРАВЛЕНИЕ 1: Объявляем свойство IntFormatter (для целых чисел на графике)
        public Func<double, string> IntFormatter { get; set; }

        public StatisticsPage()
        {
            InitializeComponent();
            _repository = new StatsRepository();

            // ИСПРАВЛЕНИЕ 1 (продолжение): Инициализируем форматтер
            // N0 означает число с 0 знаками после запятой (целое)
            IntFormatter = value => value.ToString("N0");

            // Установим дефолтные даты (последние полгода)
            DpStart.SelectedDate = DateTime.Now.AddMonths(-6);
            DpEnd.SelectedDate = DateTime.Now;

            // ВАЖНО: Устанавливаем DataContext, чтобы XAML видел IntFormatter
            DataContext = this;
        }

        private void BtnBuild_Click(object sender, RoutedEventArgs e)
        {
            if (DpStart.SelectedDate == null || DpEnd.SelectedDate == null)
            {
                MessageBox.Show("Выберите период!");
                return;
            }

            DateTime start = DpStart.SelectedDate.Value;
            DateTime end = DpEnd.SelectedDate.Value;

            if (start > end)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.");
                return;
            }

            LoadChart(start, end);
            CheckTrend(start, end);
        }

        private void LoadChart(DateTime start, DateTime end)
        {
            var data = _repository.GetStats(start, end);

            // Проверка на пустоту
            if (data.Count == 0)
            {
                MessageBox.Show("За выбранный период данных нет.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                ChartDiseases.Series = null;
                ChartDiseases.AxisX[0].Labels = null;
                return;
            }

            // 1. Настраиваем ось X (Месяцы)
            DateLabels = data.Select(x => x.MonthName).ToList();

            // Обновляем ось X
            ChartDiseases.AxisX[0].Labels = DateLabels;

            // 2. Создаем серии данных
            ChartDiseases.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Мужчины",
                    Values = new ChartValues<int>(data.Select(x => x.MaleCount)),
                    Fill = new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Синий
                    DataLabels = true
                },
                new ColumnSeries
                {
                    Title = "Женщины",
                    Values = new ChartValues<int>(data.Select(x => x.FemaleCount)),
                    Fill = new SolidColorBrush(Color.FromRgb(233, 30, 99)), // Розовый
                    DataLabels = true
                }
            };
        }

        private void CheckTrend(DateTime start, DateTime end)
        {
            // Используем новое короткое имя процедуры
            bool isDecreasing = _repository.IsTrendDecreasing(start, end);

            if (isDecreasing)
            {
                TrendBorder.Background = new SolidColorBrush(Color.FromRgb(200, 230, 201)); // Зеленый
                TxtTrendIcon.Text = "📉";
                TxtTrendResult.Text = "Заболеваемость строго снижается каждый месяц!";
                TxtTrendResult.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
            }
            else
            {
                TrendBorder.Background = new SolidColorBrush(Color.FromRgb(255, 205, 210)); // Красный
                TxtTrendIcon.Text = "📈";
                TxtTrendResult.Text = "Заболеваемость не снижается стабильно.";
                TxtTrendResult.Foreground = new SolidColorBrush(Color.FromRgb(198, 40, 40));
            }
        }

        // ИСПРАВЛЕНИЕ 2: Добавляем недостающий метод Date_Changed
        private void Date_Changed(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}