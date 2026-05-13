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
using static rpm1920.ReportService;

namespace rpm1920
{
    /// <summary>
    /// Логика взаимодействия для MovementWindow.xaml
    /// </summary>
    public partial class MovementWindow : Window
    {
        private readonly ReportService _service;
        private Movement _selectedMovement;
        public MovementWindow()
        {
            InitializeComponent();
            _service = new ReportService();
            LoadData();
        }
        private void LoadData()
        {
            var data = _service.GetAllMovements();
            dgvMovement.ItemsSource = data;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var movement = new Movement
            {
                Code = int.Parse(txtCode.Text),
                Name = txtName.Text,
                Quantity = string.IsNullOrEmpty(txtQuantity.Text) ? 0 : int.Parse(txtQuantity.Text)
            };

            try
            {
                _service.AddMovement(movement);
                LoadData();
                ClearForm();
                MessageBox.Show("✓ Деталь добавлена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMovement == null)
            {
                MessageBox.Show("Выберите запись для обновления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInput()) return;

            _selectedMovement.Name = txtName.Text;
            _selectedMovement.Quantity = string.IsNullOrEmpty(txtQuantity.Text) ? 0 : int.Parse(txtQuantity.Text);

            try
            {
                _service.UpdateMovement(_selectedMovement);
                LoadData();
                ClearForm();
                MessageBox.Show("✓ Деталь обновлена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMovement == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить деталь '{_selectedMovement.Name}'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _service.DeleteMovement(_selectedMovement.Code);
                    LoadData();
                    ClearForm();
                    MessageBox.Show("✓ Деталь удалена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void DgvMovement_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgvMovement.SelectedItem != null)
            {
                _selectedMovement = (Movement)dgvMovement.SelectedItem;
                txtCode.Text = _selectedMovement.Code.ToString();
                txtName.Text = _selectedMovement.Name;
                txtQuantity.Text = _selectedMovement.Quantity.ToString();
                txtCode.IsEnabled = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("Введите код детали", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование детали", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(txtCode.Text, out _))
            {
                MessageBox.Show("Код должен быть числом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            txtCode.Clear();
            txtName.Clear();
            txtQuantity.Clear();
            txtCode.IsEnabled = true;
            _selectedMovement = null;
        }
    }
}

