using System.Windows;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;

namespace Policlinnic.UI.Views
{
    public partial class DictionaryEditWindow : Window
    {
        private readonly DictionaryRepository _repository;
        private readonly string _type; // "Лекарства", "Болезни", "Специализации"
        private readonly object _currentItem; // Если null - добавление, иначе редактирование

        public bool IsSuccess { get; private set; } = false;

        public DictionaryEditWindow(string type, object item = null)
        {
            InitializeComponent();
            _repository = new DictionaryRepository();
            _type = type;
            _currentItem = item;

            SetupUI();
        }

        private void SetupUI()
        {
            // Настройка заголовков
            if (_type == "Лекарства")
            {
                Title = _currentItem == null ? "Новое лекарство" : "Редактирование лекарства";
                LblName.Text = "Наименование лекарства:";
                LblExtra.Text = "Зависимость от еды:";

                if (_currentItem is Medicine m)
                {
                    TxtName.Text = m.Name;
                    TxtExtra.Text = m.FoodDependency;
                }
            }
            else if (_type == "Болезни")
            {
                Title = _currentItem == null ? "Новая болезнь" : "Редактирование болезни";
                LblName.Text = "Название болезни:";
                LblExtra.Text = "Доп. примечания:";

                if (_currentItem is Illness i)
                {
                    TxtName.Text = i.Name;
                    TxtExtra.Text = i.Notes;
                }
            }
            else if (_type == "Специализации")
            {
                Title = _currentItem == null ? "Новая специализация" : "Редактирование специализации";
                LblName.Text = "Название специализации:";

                // У специализаций только одно поле, скрываем второе
                PanelExtra.Visibility = Visibility.Collapsed;

                if (_currentItem is Specialization s)
                {
                    TxtName.Text = s.Name;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Введите название!");
                return;
            }

            try
            {
                if (_type == "Лекарства")
                {
                    if (_currentItem is Medicine m) // Редактирование
                        _repository.UpdateMedicine(m.ID, TxtName.Text, TxtExtra.Text);
                    else // Добавление
                        _repository.AddMedicine(TxtName.Text, TxtExtra.Text);
                }
                else if (_type == "Болезни")
                {
                    if (_currentItem is Illness i)
                        _repository.UpdateIllness(i.ID, TxtName.Text, TxtExtra.Text);
                    else
                        _repository.AddIllness(TxtName.Text, TxtExtra.Text);
                }
                else if (_type == "Специализации")
                {
                    if (_currentItem is Specialization s)
                        _repository.UpdateSpecialization(s.ID, TxtName.Text);
                    else
                        _repository.AddSpecialization(TxtName.Text);
                }

                IsSuccess = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}