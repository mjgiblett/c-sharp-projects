using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Finance_Manager.cs;

namespace Finance_Manager
{
    /// <summary>
    /// Interaction logic for TransactionManagerWindow.xaml
    /// </summary>
    public partial class TransactionManagerWindow : Window
    {
        private readonly TransactionManager transactionManager;

        public TransactionManagerWindow(TransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;

            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            dataGridTransactions.ItemsSource = transactionManager.Transactions;

            datePicker.SelectedDate = null;
            datePicker.Focus();
            txtDescription.Clear();
            txtAmount.Clear();
            buttonSave.Content = "Save";
        }

        private void ButtonCancel (object sender, RoutedEventArgs e)
        {

        }

        private void ButtonDeleteAll(object sender, RoutedEventArgs e)
        {

        }
        private void ButtonImport(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonSave(object sender, RoutedEventArgs e)
        {

        }

        private void dataGridTransaction_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void ValidateTextAsDecimal(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[0-9.]");
            e.Handled = !regex.IsMatch(e.Text);
        }


    }
}
