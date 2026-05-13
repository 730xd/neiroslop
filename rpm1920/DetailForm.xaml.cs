using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using rpm1920.Models;

namespace rpm1920
{
    /// <summary>
    /// Логика взаимодействия для DetailForm.xaml
    /// </summary>
    public partial class DetailForm : Window
    {
        private readonly ReportService _service;
        private Movement _movement;
        private PriceList _price;
        private bool _isEditMode;
        public DetailForm()
        {
            InitializeComponent();
            _service = new ReportService();
            _isEditMode = false;
            lblTitle.Text = "➕ ДОБАВЛЕНИЕ НОВОЙ ДЕТАЛИ";
            txtCode.IsEnabled = true;
        }
        public DetailForm(Movement movement, PriceList price)
        {
            InitializeComponent();
            _service = new ReportService();
            _movement = movement;
            _price = price;
            _isEditMode = true;
            lblTitle.Text = "✏️ РЕДАКТИРОВАНИЕ ДЕТАЛИ";
            txtCode.IsEnabled = false; // Код нельзя менять при редактировании
            LoadData();
        }

        private void LoadData()
        {
            txtCode.Text = _movement.Code.ToString();
            txtName.Text = _movement.Name;
            txtQuantity.Text = _movement.Quantity.ToString();
            txtPrice.Text = _price != null ? _price.Price.ToString("F2") : "0";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка валидации
            if (!ValidateInput()) return;

            try
            {
                if (_isEditMode)
                {
                    // РЕДАКТИРОВАНИЕ
                    _movement.Name = txtName.Text;
                    _movement.Quantity = int.Parse(txtQuantity.Text);
                    _service.UpdateMovement(_movement);

                    if (_price != null)
                    {
                        _price.Price = decimal.Parse(txtPrice.Text);
                        _service.UpdatePrice(_price);
                    }
                    else
                    {
                        var newPrice = new PriceList
                        {
                            Code = _movement.Code,
                            Price = decimal.Parse(txtPrice.Text)
                        };
                        _service.AddPrice(newPrice);
                    }
                    MessageBox.Show("✓ Данные успешно обновлены", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // ДОБАВЛЕНИЕ
                    int code = int.Parse(txtCode.Text);

                    // Проверка на существование
                    if (_service.GetMovementByCode(code) != null)
                    {
                        MessageBox.Show("Деталь с таким кодом уже существует!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var movement = new Movement
                    {
                        Code = code,
                        Name = txtName.Text,
                        Quantity = int.Parse(txtQuantity.Text)
                    };
                    _service.AddMovement(movement);

                    var price = new PriceList
                    {
                        Code = code,
                        Price = decimal.Parse(txtPrice.Text)
                    };
                    _service.AddPrice(price);

                    MessageBox.Show("✓ Деталь успешно добавлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (!_isEditMode && string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("Введите код детали", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование детали", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                MessageBox.Show("Введите количество", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Введите цену", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            int qty;
            if (!int.TryParse(txtQuantity.Text, out qty) || qty < 0)
            {
                MessageBox.Show("Количество должно быть положительным числом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            decimal price;
            if (!decimal.TryParse(txtPrice.Text, out price) || price < 0)
            {
                MessageBox.Show("Цена должна быть положительным числом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!_isEditMode)
            {
                int code;
                if (!int.TryParse(txtCode.Text, out code) || code <= 0)
                {
                    MessageBox.Show("Код должен быть положительным числом", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

}

