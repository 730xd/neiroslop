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

namespace rpm1920
{
    /// <summary>
    /// Логика взаимодействия для DatePickerDialog.xaml
    /// </summary>
    public partial class DatePickerDialog : Window
    {
        public DateTime SelectedDate { get; private set; }
        public DatePickerDialog()
        {
            InitializeComponent();
            dpDate.SelectedDate = DateTime.Now;
        }
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (dpDate.SelectedDate.HasValue)
            {
                SelectedDate = dpDate.SelectedDate.Value;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Выберите дату", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
