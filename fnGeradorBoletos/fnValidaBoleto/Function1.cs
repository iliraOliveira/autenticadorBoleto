using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fnValidaBoleto;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function("barcode-validate")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody); 
        string codigoBarras = data?.codigoBarras;

        if (string.IsNullOrEmpty(codigoBarras) || codigoBarras.Length != 44)
        {
            var result = new { valido = false, mensagem = "Código de barras inválido ou não fornecido." };
            return new BadRequestObjectResult(result);
        }

        string datePart = codigoBarras.Substring(3, 8);
        if (!DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime vencimento))
        {
            var result = new { valido = false, mensagem = "Data de vencimento inválida no código de barras." };
            return new BadRequestObjectResult(result);
        }

        var resultValido = new { valido = true, mensagem = "Código de barras válido.", venciamentoBoleto = vencimento.ToString("dd-MM-yyyy") };

        return new OkObjectResult(resultValido);
    }
}