using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MarketDataApp.Models;

namespace MarketDataApp
{
    public partial class MainWindow : Window
    {
        private HttpClient client;

        public MainWindow()
        {
            InitializeComponent();
            client = new HttpClient();

            cmbIndicatorType.SelectedItem = cmbIndicatorType.Items[0];

            // Definir limites de datas
            dpStartDate.DisplayDateStart = new DateTime(2005, 1, 1);
            dpStartDate.DisplayDateEnd = DateTime.Today;
            dpEndDate.DisplayDateStart = new DateTime(2005, 1, 1);
            dpEndDate.DisplayDateEnd = DateTime.Today;

        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            dgMarketData.Visibility = Visibility.Collapsed;
            progressBar.Visibility = Visibility.Visible;
            string indicatorType = (cmbIndicatorType.SelectedItem as ComboBoxItem)?.Content.ToString();
            DateTime startDate = dpStartDate.SelectedDate.Value;
            DateTime endDate = dpEndDate.SelectedDate.Value;

            try
            {
                var data = await GetDataAsync(indicatorType, startDate, endDate);
                if (data != null && data.Any())
                {
                    dgMarketData.ItemsSource = data;
                    dgMarketData.Visibility = Visibility.Visible;
                    btnExportToCSV.Visibility = Visibility.Visible; // Mostrar botao de export csv
                }
                else
                {
                    dgMarketData.Visibility = Visibility.Collapsed;
                    btnExportToCSV.Visibility = Visibility.Collapsed; // Esconde botao de export csv
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Oculta o ProgressBar
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<List<MarketData>> GetDataAsync(string indicatorType, DateTime startDate, DateTime endDate)
        {
            string apiPath = "ExpectativaMercadoMensais";
            if (indicatorType == "Selic")
                apiPath = "ExpectativasMercadoSelic";

            string api = $"https://olinda.bcb.gov.br/olinda/servico/Expectativas/versao/v1/odata/{apiPath}?%24orderby=Indicador%2CData&%24filter=Indicador%20eq%20'{indicatorType}'%20and%20Data%20gt%20'{startDate.ToString("yyyy-MM-dd")}'%20and%20Data%20lt%20'{endDate.ToString("yyyy-MM-dd")}' ";
            HttpResponseMessage response = await client.GetAsync(api);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ApiResponse>(responseBody);
            var filteredValues = responseObject.value;


            List<MarketData> marketData = new List<MarketData>();

            foreach (var value in filteredValues)
            {
                MarketData marketItem = new MarketData
                {
                    Indicador = value.Indicador,
                    Data = value.Data,
                    Media = value.Media,
                    Mediana = value.Mediana,
                    DesvioPadrao = value.DesvioPadrao,
                    Minimo = value.Minimo,
                    Maximo = value.Maximo,
                    baseCalculo = value.baseCalculo
                };

                if (indicatorType == "Selic")
                    marketItem.Reuniao = value.Reuniao;
                else
                    marketItem.DataReferencia = value.DataReferencia;

                marketData.Add(marketItem);
            }

            return marketData;
        }


        private void cmbIndicatorType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void dgMarketData_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            var data = dgMarketData.ItemsSource as List<MarketData>;
            if (data != null && data.Any())
            {
                StringBuilder csvContent = new StringBuilder();
                csvContent.AppendLine("Indicador,Data,DataReferencia,Reuniao,Media,Mediana,DesvioPadrao,Minimo,Maximo,baseCalculo");
                foreach (var item in data)
                {
                    csvContent.AppendLine($"{item.Indicador},{item.Data},{item.DataReferencia},{item.Reuniao},{item.Media},{item.Mediana},{item.DesvioPadrao},{item.Minimo},{item.Maximo},{item.baseCalculo}");
                }

                string fileName = "MarketData.csv"; // Nome do arquivo CSV
                string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Diretório Meus Documentos
                string filePath = Path.Combine(directory, fileName); // Caminho completo do arquivo CSV
                File.WriteAllText(filePath, csvContent.ToString());

                MessageBox.Show($"Data exported to {filePath} successfully!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No data available to export!", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }

    public class ApiResponse
    {
        public List<MarketData> value { get; set; }
    }

}
