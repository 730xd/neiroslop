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
    /// Логика взаимодействия для MonthReportDialog.xaml
    /// </summary>
    public partial class MonthReportDialog : Window
    {
        public int SelectedMonth { get; private set; }
        public int SelectedYear { get; private set; }
        public MonthReportDialog()
        {
            InitializeComponent();
            LoadYears();
        }
        private void LoadYears()
        {
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear - 2; year <= currentYear + 1; year++)
            {
                cbYear.Items.Add(year);
            }
            cbYear.SelectedItem = 2025;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (cbMonth.SelectedItem is ComboBoxItem monthItem)
            {
                SelectedMonth = Convert.ToInt32(monthItem.Tag);
                SelectedYear = (int)cbYear.SelectedItem;
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

