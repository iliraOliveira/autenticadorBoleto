using Azure.Messaging.ServiceBus;
using BarcodeStandard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fnGeradorBoletos
{
    public class GeradorCodigoBarras
    {
        private readonly ILogger<GeradorCodigoBarras> _logger;
        private readonly string _connectionString;
        private readonly string _queueName = "boleto-queue";

        public GeradorCodigoBarras(ILogger<GeradorCodigoBarras> logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        }

        [Function("barcode-generate")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                string valor = data?.valor;
                string dataVencimento = data?.dataVencimento;

                if (string.IsNullOrEmpty(valor) || string.IsNullOrEmpty(dataVencimento))
                {
                    _logger.LogError("Valor or DataVencimento is null or empty.");
                    return new BadRequestObjectResult("Valor and DataVencimento are required.");
                }

                if (!DateTime.TryParseExact(dataVencimento, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime vencimentoDateTime))
                {
                    _logger.LogError("Invalid DataVencimento format.");
                    return new BadRequestObjectResult("Invalid DataVencimento format.");
                }

                string dataVencimentoFormatted = vencimentoDateTime.ToString("yyyyMMdd");

                if (!decimal.TryParse(valor, out decimal valorDecimal) || valorDecimal <= 0)
                {
                    _logger.LogError("Valor must be a positive decimal number.");
                    return new BadRequestObjectResult("Valor must be a positive decimal number.");
                }

                int valorCentavos = (int)(valorDecimal * 100);
                string valorFormatado = valorCentavos.ToString("D8");

                string banckCode = "001"; // Banco do Brasil
                string basecode = $"{banckCode}{dataVencimentoFormatted}{valorFormatado}";
                // Preenchimento do código de barras para total de 44 caracteres
                string barcodeData = basecode.Length < 44 ? basecode.PadRight(44, '0') : basecode.Substring(0,44);
                _logger.LogInformation($"Generated barcode data: {barcodeData}");

                Barcode barcode = new Barcode();
                var barcodeImage = barcode.Encode(BarcodeStandard.Type.Code128, barcodeData);

                if (barcodeImage == null)
                {
                    _logger.LogError("Failed to generate barcode image.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                using (var encodeData = barcodeImage.Encode(SkiaSharp.SKEncodedImageFormat.Png,100))
                {
                    byte[] imageBytes = encodeData.ToArray();
                    string base64Image = Convert.ToBase64String(imageBytes);
                    var resultObject = new
                    {
                        barcode = barcodeData,
                        valorOriginal = valorDecimal,
                        DataVencimento = vencimentoDateTime.ToString("dd-MM-yyyy"),
                        ImagemBase64 = base64Image
                    };
                    await SendFileFallback(resultObject, _connectionString, _queueName);
                    return new OkObjectResult(resultObject);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the barcode.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            
        }

        private async Task SendFileFallback(object resultObject, string connectionString, string queueName)
        {
            await using var client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(queueName);
            try
            {
                string message = JsonConvert.SerializeObject(resultObject);
                await sender.SendMessageAsync(new ServiceBusMessage(message));
                _logger.LogInformation("Message sent to the queue successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to the queue.");
                throw;
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
