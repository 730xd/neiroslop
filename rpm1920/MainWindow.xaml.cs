using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using rpm1920.Models;
using static rpm1920.ReportService;

namespace rpm1920
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ReportService _service;
        private DetailViewModel _selectedDetail;
        public MainWindow()
        {
            InitializeComponent();
            _service = new ReportService();
            LoadInitialData();
        }
        private void LoadInitialData()
        {
            try
            {
                var data = _service.SearchDetails("");
                dgvData.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void DgvData_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgvData.SelectedItem != null)
            {
                _selectedDetail = (DetailViewModel)dgvData.SelectedItem;
            }
        }

        // ========== CRUD ОПЕРАЦИИ ==========

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DetailForm();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                LoadInitialData();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDetail == null)
            {
                MessageBox.Show("Выберите запись для редактирования", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var movement = _service.GetMovementByCode(_selectedDetail.Code);
            var price = _service.GetPriceByCode(_selectedDetail.Code);

            var dialog = new DetailForm(movement, price);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                LoadInitialData();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDetail == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить деталь '{_selectedDetail.Name}'? Все связанные данные будут удалены!",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _service.DeleteDetailWithRelations(_selectedDetail.Code);
                    LoadInitialData();
                    MessageBox.Show("Деталь и все связанные данные удалены", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            LoadInitialData();
        }

        // ========== ПОИСК ==========

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            var data = _service.SearchDetails(searchText);
            dgvData.ItemsSource = data;

            if (!data.Any() && !string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Детали не найдены", "Поиск",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ========== ПР19 ЗАДАНИЕ 2: Детали за месяц ==========

        private void BtnMonthReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MonthReportDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                var data = _service.GetDetailsByMonth(dialog.SelectedMonth, dialog.SelectedYear);

                if (data.Any())
                {
                    dgvData.ItemsSource = data;
                    MessageBox.Show($"Поступление за {dialog.SelectedMonth}.{dialog.SelectedYear}\nНайдено: {data.Count} видов деталей",
                        "Отчет ПР19(2)", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Нет поступлений за указанный месяц", "Отчет",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // ========== ПР19 ЗАДАНИЕ 3: Стоимость по требованиям ==========

        private void BtnCostReport_Click(object sender, RoutedEventArgs e)
        {
            var data = _service.GetCostByRequests();

            if (data.Any())
            {
                dgvData.ItemsSource = data;
                MessageBox.Show($"Найдено требований: {data.Count}\nОбщая стоимость: {data.Sum(x => x.TotalCost):C2}",
                    "Отчет ПР19(3)", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Нет данных о выдаче", "Отчет",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ========== ПР22 ЗАДАНИЕ 1: Остаток деталей на складе ==========

        private void BtnStockBalance_Click(object sender, RoutedEventArgs e)
        {
            var data = _service.GetStockBalance();
            dgvData.ItemsSource = data;
            MessageBox.Show($"Остаток деталей на складе\nВсего позиций: {data.Count}\nОбщий остаток: {data.Sum(x => x.Balance)} шт.",
                "Отчет ПР22(1)", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ========== ПР22 ЗАДАНИЕ 2: Движение детали на дату ==========

        private void BtnMovementByDate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DatePickerDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                var data = _service.GetMovementByDate(dialog.SelectedDate);

                if (data.Any())
                {
                    dgvData.ItemsSource = data;
                    MessageBox.Show($"Движение деталей за {dialog.SelectedDate:dd.MM.yyyy}\nЗаписей: {data.Count}",
                        "Отчет ПР22(2)", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Нет движения за {dialog.SelectedDate:dd.MM.yyyy}", "Отчет",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // ========== ПР22 ЗАДАНИЕ 4: Стоимость деталей на складе ==========

        private void BtnStockValue_Click(object sender, RoutedEventArgs e)
        {
            var data = _service.GetStockValue();
            dgvData.ItemsSource = data;
            decimal total = data.Sum(x => x.TotalValue);
            MessageBox.Show($"Общая стоимость деталей на складе: {total:C2}\nВсего позиций: {data.Count}",
                "Отчет ПР22(4)", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ========== ПР22 ЗАДАНИЕ 5: Самая выдаваемая деталь ==========

        private void BtnMostIssued_Click(object sender, RoutedEventArgs e)
        {
            var data = _service.GetMostIssuedDetail();

            if (data != null)
            {
                MessageBox.Show($"🏆 САМАЯ ВЫДАВАЕМАЯ ДЕТАЛЬ\n\n" +
                    $"Наименование: {data.DetailName}\n" +
                    $"Всего выдано: {data.TotalIssued} шт.\n\n" +
                    $"✓ Запрос выполнен с помощью LINQ",
                    "Отчет ПР22(5)", MessageBoxButton.OK, MessageBoxImage.Information);

                // Показываем все детали в таблице
                var allDetails = _service.GetStockBalance();
                dgvData.ItemsSource = allDetails;
            }
            else
            {
                MessageBox.Show("Нет данных о выдаче", "Отчет",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}