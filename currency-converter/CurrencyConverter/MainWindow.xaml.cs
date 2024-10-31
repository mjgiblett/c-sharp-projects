using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CurrencyConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Root root = new Root();

        SqlConnection connection = new SqlConnection();
        SqlCommand command = new SqlCommand();
        SqlDataAdapter adapter = new SqlDataAdapter();

        private int CurrencyID = 0;

        public class Root
        {
            public Dictionary<string, float> rates;
            public long timestamp;
            public string license; 
        }

        public MainWindow()
        {
            InitializeComponent();
            BindCurrency();
            GetData();
        }



        private async void GetValues()
        {
            string url = "https://openexchangerates.org/api/latest.json?app_id=";
            root = await GetData<Root>(url);
            try
            {
                Connect();
                foreach (KeyValuePair<string, float> entry in root.rates)
                {
                    command = new SqlCommand("IF NOT EXISTS (SELECT CurrencyName FROM Currency WHERE CurrencyName = @CurrencyName) "
                        + "BEGIN INSERT INTO Currency(Amount, CurrencyName) VALUES(@Amount, @CurrencyName) END", connection);
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Amount", entry.Value);
                    command.Parameters.AddWithValue("@CurrencyName", entry.Key);
                    command.ExecuteNonQuery();
                }
                connection.Close();
                ClearMaster();
                MessageBox.Show("Data Successfully Saved", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exc)
            {

                MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static async Task<Root> GetData<T>(string url)
        {
            var myRoot = new Root();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(1);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var ResponseString = await response.Content.ReadAsStringAsync();
                        var ResponseObject = JsonConvert.DeserializeObject<Root>(ResponseString);
                        return ResponseObject;
                    }
                    return myRoot;
                }
            }
            catch
            {
                return myRoot;
            }
        }

        public void Connect()
        {
            String connector = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            connection = new SqlConnection(connector);
            connection.Open();
        }

        private void BindCurrency()
        {
            Connect();
            DataTable dataTable = new DataTable();
            command = new SqlCommand("SELECT * FROM Currency", connection);
            //command = new SqlCommand("SELECT Amount, CurrencyName FROM Currency", connection);
            command.CommandType = CommandType.Text; 
            adapter = new SqlDataAdapter(command);
            adapter.Fill(dataTable);

            DataRow newRow = dataTable.NewRow();
            newRow["Id"] = 0;
            newRow["Amount"] = 0;
            newRow["CurrencyName"] = "--Select--";
            dataTable.Rows.InsertAt(newRow, 0);

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                cmbFromCurrency.ItemsSource = dataTable.DefaultView;
                cmbToCurrency.ItemsSource = dataTable.DefaultView;
            }

            connection.Close();

            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Amount";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Amount";
            cmbToCurrency.SelectedIndex = 0;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+\\.?[^0-9]+?");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double ConvertedValue;

            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency.Focus();
                return;
            }
            else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency.Focus();
                return; 
            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency.Focus();
                return;
            }

            if (cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                ConvertedValue = double.Parse(txtCurrency.Text);
                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
            else
            {
                ConvertedValue = double.Parse(cmbFromCurrency.SelectedValue.ToString()) * 
                    double.Parse(txtCurrency.Text) /
                    double.Parse(cmbToCurrency.SelectedValue.ToString());
                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }

        private void ClearControls()
        {
            txtCurrency.Clear();
            if (cmbFromCurrency.Items.Count > 0)
            {
                cmbFromCurrency.SelectedIndex = 0;
            }
            if (cmbToCurrency.Items.Count > 0)
            {
                cmbToCurrency.SelectedIndex = 0;
            }
            lblCurrency.Content = "";
            txtCurrency.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == "")
                {
                    MessageBox.Show("Please Enter Amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == "")
                {
                    MessageBox.Show("Please Enter Currency Name", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrencyName.Focus();
                    return;
                }
                else
                {
                    if (CurrencyID > 0)
                    {
                        if (MessageBox.Show("Are you sure you want to update?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            Connect();
                            DataTable dataTable = new DataTable();
                            command = new SqlCommand("UPDATE Currency SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", connection);
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@Id", CurrencyID);
                            command.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            command.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            command.ExecuteNonQuery();
                            connection.Close();

                            MessageBox.Show("Data Successfully Updated", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        if (MessageBox.Show("Are you sure you want to update?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            Connect();
                            DataTable dataTable = new DataTable();
                            command = new SqlCommand("INSERT INTO Currency(Amount, CurrencyName) VALUES(@Amount, @CurrencyName)", connection);
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            command.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            command.ExecuteNonQuery();
                            connection.Close();

                            MessageBox.Show("Data Successfully Saved", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    ClearMaster();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearMaster()
        {
            try
            {
                txtAmount.Clear();
                txtCurrencyName.Clear();
                Save.Content = "Save";
                GetData();
                CurrencyID = 0;
                BindCurrency();
                txtAmount.Focus();
            }
            catch (Exception exc)
            {

                MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetData()
        {
            Connect();
            DataTable dataTable = new DataTable();
            command = new SqlCommand("SELECT * FROM Currency", connection);
            command.CommandType= CommandType.Text;
            adapter = new SqlDataAdapter(command);
            adapter.Fill(dataTable);
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                dgvCurrency.ItemsSource = dataTable.DefaultView;
            }
            else
            {
                dgvCurrency.ItemsSource = null; 
            }
            connection.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMaster();
            }
            catch (Exception exc)
            {

                MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid dataGrid = (DataGrid)sender;
                DataRowView selectedRow = dataGrid.CurrentItem as DataRowView;

                if (selectedRow != null)
                {
                    if (dgvCurrency.Items.Count > 0)
                    {
                        if (dataGrid.SelectedCells.Count > 0)
                        {
                            CurrencyID = Int32.Parse(selectedRow["Id"].ToString());

                            if (dataGrid.SelectedCells[0].Column.DisplayIndex == 0)
                            {
                                txtAmount.Text = selectedRow["Amount"].ToString();
                                txtCurrencyName.Text = selectedRow["CurrencyName"].ToString();
                                Save.Content = "Update";
                            }
                            if (dataGrid.SelectedCells[0].Column.DisplayIndex == 1)
                            {
                                if (MessageBox.Show("Are you sure you want to delete?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    Connect();
                                    command = new SqlCommand("DELETE FROM Currency WHERE Id = @Id", connection);
                                    command.CommandType = CommandType.Text;
                                    command.Parameters.AddWithValue("@Id", CurrencyID);
                                    command.ExecuteNonQuery();
                                    connection.Close();

                                    MessageBox.Show("Data Successfully Deleted", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ClearMaster();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Google_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to get currencies from Open Exchange Rates?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                GetValues();
            }
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to delete all?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Connect();
                    command = new SqlCommand("DELETE FROM Currency", connection);
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Id", CurrencyID);
                    command.ExecuteNonQuery();
                    connection.Close();

                    MessageBox.Show("Data Successfully Deleted", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearMaster();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
