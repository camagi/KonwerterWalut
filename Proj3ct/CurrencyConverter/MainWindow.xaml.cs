using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace CurrencyConverter
{
    public partial class MainWindow : Window
    {
        private const string ApiBaseUrl = "https://v6.exchangerate-api.com/v6/b144c6c5a86a9846ed10f4a1/latest/USD";

        private ExchangeDBcontext dBcontext = new();

        public MainWindow()
        {
            InitializeComponent();

            if (this.dBcontext.Database.GetPendingMigrations().Any())
            {
                this.dBcontext.Database.Migrate();
            }

            ClearControls();
            FetchCurrencies();
            GetCurrencies();
        }

        private void GetCurrencies()
        {
                    if (this.dBcontext.Exchanges.Any())
                    {

                        List<Exchange> vals = this.dBcontext.Exchanges.ToList();

                        DataTable dtCurrency = new DataTable();
                        dtCurrency.Columns.Add("Text");
                        dtCurrency.Columns.Add("Value");

                        dtCurrency.Rows.Add("USD", 1);

                        foreach (var rate in vals)
                        {
                            dtCurrency.Rows.Add(rate.Text, rate.Value);
                        }

                        cmbFromCurrency.ItemsSource = dtCurrency.DefaultView;
                        cmbFromCurrency.DisplayMemberPath = "Text";
                        cmbFromCurrency.SelectedValuePath = "Value";
                        cmbFromCurrency.SelectedIndex = 0;

                        cmbToCurrency.ItemsSource = dtCurrency.DefaultView;
                        cmbToCurrency.DisplayMemberPath = "Text";
                        cmbToCurrency.SelectedValuePath = "Value";
                        cmbToCurrency.SelectedIndex = 0;
                    }
        }

        private async void FetchCurrencies()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = ApiBaseUrl;
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var ratesJson = JObject.Parse(responseBody);

                        foreach (JProperty rates in ratesJson["conversion_rates"])
                        {
                            var searchResult = this.dBcontext.Exchanges.SingleOrDefault(r => r.Text == rates.Name);
                            if (searchResult != null)
                            {
                                searchResult.Text = rates.Name;
                                searchResult.Value = float.Parse(rates.Value.ToString());
                            }
                            else
                            {
                                this.dBcontext.Add<Exchange>(new Exchange()
                                    {
                                        Text = rates.Name,
                                        Value = float.Parse(rates.Value.ToString())
                                    }
                                );
                            }
                           
                        }
                        this.dBcontext.SaveChanges();
                    }
                    else
                    {
                        MessageBox.Show("Failed to fetch currency data. Please try again later.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async void Convert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtCurrency.Text))
                {
                    MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrency.Focus();
                    return;
                }

                string fromCurrency = cmbFromCurrency.Text;
                string toCurrency = cmbToCurrency.Text;
                double amount = double.Parse(txtCurrency.Text);

                var fromRate = this.dBcontext.Exchanges.SingleOrDefault(r => r.Text == fromCurrency).Value;
                var toRate = this.dBcontext.Exchanges.SingleOrDefault(r => r.Text == toCurrency).Value;
                double convertedValue = (amount / fromRate) * toRate;

                lblCurrency.Content = $"{toCurrency} {convertedValue:N3}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }

        private void ClearControls()
        {
            txtCurrency.Text = string.Empty;
            cmbFromCurrency.SelectedIndex = 0;
            cmbToCurrency.SelectedIndex = 0;
            lblCurrency.Content = "";
            txtCurrency.Focus();
        }

        private void txtCurrency_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[0-9.]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

    }
}
