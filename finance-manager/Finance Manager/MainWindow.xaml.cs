using Finance_Manager.cs;
using Microsoft.Win32;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace Finance_Manager
{
    public partial class MainWindow : Window
    {
        readonly Dictionary<string, string> transactionCommands = new Dictionary<string, string>
        {
            { "insert", "INSERT INTO [transaction](Date, AccountID, Description, Amount) " +
                "VALUES(@Date, @AccountID, @Description, @Amount)" },

            { "update", "UPDATE [transaction] SET Date = @Date, AccountID = @AccountID, " +
                "Description = @Description, Amount = @Amount WHERE TransactionID = @TransactionID" },

            { "delete", "DELETE FROM [transaction] WHERE TransactionID = @TransactionID" },
            { "delete all", "DELETE FROM [transaction]" }
        };

        int? selectedTransactionID;

        internal readonly BankAccountManager bankAccountManager = new BankAccountManager();
        internal readonly TransactionManager transactionManager = new TransactionManager();

        TransactionManagerWindow window;

        public MainWindow()
        {
            InitializeComponent();
            SetReset();
        }

        private void SetReset()
        {
            transactionManager.Transactions = transactionManager.GetTransactionsFromSQL("SELECT [transaction].*, [account].[Name] FROM [transaction] INNER JOIN [account] ON [transaction].AccountID = [account].AccountID");

            bankAccountManager.Accounts = bankAccountManager.GetAccountsFromSQL("SELECT * FROM [account]");

            selectedTransactionID = null;
            datePicker.SelectedDate = null;
            datePicker.Focus();
            txtDescription.Clear();
            txtAmount.Clear();
            Save.Content = "Save";

            FillDataGrids();
            FillComboBox();
            FillChart();
            
        }

        private void FillChart()
        {
            Transaction[] selectedTransactions;
            int selectedAccountID;

            if (comboBoxChartAccount.SelectedValue == null) { selectedAccountID = 0; }
            else { selectedAccountID = int.Parse(comboBoxChartAccount.SelectedValue.ToString()); }

            if (selectedAccountID > 0)
            {
                selectedTransactions = transactionManager.Transactions.Where(transaction => transaction.AccountID == selectedAccountID).ToArray();
            }
            else
            {
                selectedTransactions = transactionManager.Transactions.Where(transaction => !transaction.Description.Contains("Internal")).ToArray();
            }
            
            if (selectedTransactions.Count() == 0) { return; }

            DateAmountCollection.Balance balance = new DateAmountCollection.Balance(selectedTransactions);
            DateAmountCollection.Credit credit = new DateAmountCollection.Credit(selectedTransactions);
            DateAmountCollection.Debit debit = new DateAmountCollection.Debit(selectedTransactions);
            
            if (checkBoxBalance.IsChecked == true) { chart.Series.Add(balance.ToLineSeries(Color.FromRgb(0, 0, 255))); }
            if (checkBoxCredit.IsChecked == true) { chart.Series.Add(credit.ToLineSeries(Color.FromRgb(0, 255, 0))); }
            if (checkBoxDebit.IsChecked == true) { chart.Series.Add(debit.ToLineSeries(Color.FromRgb(255, 0, 0))); }
        }

        private void FillDataGrids()
        {
            dataGridAccount.ItemsSource = bankAccountManager.Accounts;
            dataGridTransaction.ItemsSource = transactionManager.Transactions;
        }

        private void FillComboBox()
        {
            comboBoxAccount.ItemsSource = bankAccountManager.Accounts;
            comboBoxAccount.DisplayMemberPath = "Name";
            comboBoxAccount.SelectedValuePath = "AccountID";
            comboBoxAccount.SelectedIndex = 0;

            List<BankAccount> bankAccounts = new List<BankAccount>
            {
                new BankAccount(0, "All"),
            };

            bankAccounts.AddRange(bankAccountManager.Accounts);

            comboBoxChartAccount.ItemsSource = bankAccounts;
            comboBoxChartAccount.DisplayMemberPath = "Name";
            comboBoxChartAccount.SelectedValuePath = "AccountID";
            comboBoxChartAccount.SelectedIndex = 0;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All Files| *.*"
            };
            if ((bool)openFileDialog.ShowDialog())
            {
                using (StreamReader streamReader = new StreamReader(openFileDialog.FileName))
                {
                    DataTable dataTable = new DataTable()
                    {
                        TableName = "[transaction]", 
                        Columns = { 
                            new DataColumn("TransactionID", typeof(int)),
                            new DataColumn("Date", typeof(DateTime)),
                            new DataColumn("AccountID", typeof(int)),
                            new DataColumn("Description", typeof(string)),
                            new DataColumn("Amount", typeof(decimal)),
                        }
                    };

                    streamReader.ReadLine(); //Remove headings
                    string[] rows = streamReader.ReadToEnd().Split('\n');
                    Array.Reverse(rows);
                    Regex regex = new Regex("\".*\"");

                    int numberFailed = 0;

                    foreach (string row in rows)
                    {
                        if (row.Trim() == "") continue;
                        bool isMatch = regex.IsMatch(row);
                        string match = regex.Match(row).ToString();
                        string[] values = isMatch ? row.Replace(match, "").Split(',') : row.Split(',');
                        if (values.Count() != 5) { numberFailed++; continue; };

                        if (!DateTime.TryParse(values[0], out DateTime date)) { numberFailed++; continue; };

                        int? accountID = null;
                        try
                        {
                            foreach (BankAccount account in bankAccountManager.Accounts)
                            {
                                if (account.AccountNumber == int.Parse(values[1])) 
                                { 
                                    accountID = account.AccountID; 
                                }
                            }
                            if (!accountID.HasValue) { numberFailed++; continue; }
                        }
                        catch { numberFailed++; continue; };
                        
                        string description = isMatch ? match.Replace("\"", "") : values[2];

                        decimal credit = decimal.TryParse(values[3], out decimal valueCredit) ? valueCredit : 0;
                        decimal debit = decimal.TryParse(values[4], out decimal valueDebit) ? valueDebit : 0;
                        decimal amount = credit + debit;

                        dataTable.Rows.Add(null, date, accountID, description, amount);
                    }

                    SqlHelper.BulkWriteToServer(dataTable);

                    if (numberFailed > 0) { MessageBox.Show($"{numberFailed} transactions failed.", "Information", MessageBoxButton.OK, MessageBoxImage.Information); };
                }
            }
            SetReset();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!DateTime.TryParse(datePicker.SelectedDate.ToString(), out DateTime date))
            {
                MessageBox.Show("Please enter a date.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Please enter an amount as a valid decimal.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Transaction transaction = new Transaction(int.Parse(comboBoxAccount.SelectedValue.ToString()), 
                date, amount, txtDescription.Text);

            if (selectedTransactionID != null)
            {
                transaction.TransactionID = (int)selectedTransactionID;
                SqlHelper.ExecuteNonQuery(transactionCommands["update"], transaction.ToSqlParameterArray());
            }
            else
            {
                SqlHelper.ExecuteNonQuery(transactionCommands["insert"], transaction.ToSqlParameterArray());
            }
            SetReset();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SetReset();
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SqlHelper.ExecuteNonQuery(transactionCommands["delete all"]);
                SetReset();
            }
        }

        private void DataGridTransaction_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;
            Transaction transaction = (Transaction)dataGrid.CurrentItem;

            if (e.AddedCells.Count() > 0)
            {
                int transactionID = transaction.TransactionID;

                if (dataGrid.SelectedCells[0].Column.DisplayIndex == 0)
                {
                    if (MessageBox.Show("Are you sure you want to delete?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {

                        SqlParameter parameter = new SqlParameter("TransactionID", transactionID);

                        SqlHelper.ExecuteNonQuery(transactionCommands["delete"], parameter);
                        SetReset();
                    }
                }
                else if (dataGrid.SelectedCells[0].Column.DisplayIndex == 1)
                {
                    int account = transaction.AccountID;

                    comboBoxAccount.SelectedValue = account;

                    datePicker.SelectedDate = transaction.Date;

                    txtDescription.Text = transaction.Description;
                    txtAmount.Text = transaction.Amount.ToString();
                    Save.Content = "Update";
                    selectedTransactionID = transactionID;
                }
            }
        }
        
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[0-9.]");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void ChangeOverviewChart(object sender, RoutedEventArgs e)
        {
            chart.Series.Clear();
            FillChart();
        }

        private void buttonTransactionManager_Click(object sender, RoutedEventArgs e)
        {
            window?.Close();
            window = new TransactionManagerWindow(transactionManager);
            window.Show();
        }
    }
}
