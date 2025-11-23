using ActivityProvider.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text;

namespace ActivityProvider.Endpoints
{
    public static class ActivityProviderEndpoints
    {
        public static void MapActivityProviderEndpoints(this WebApplication app)
        {
            app.MapGet("/", GetConfigFile);
            app.MapGet("/index", GetConfigFileJson);
            app.MapGet("/config-translate", GetConfig);
            app.MapGet("/json-params-translate", GetJsonParamsConfig);
            app.MapGet("/deploy-translate", GetDeploy);
            app.MapPost("/analytics-translate", GetAnalyticsConfig);
            app.MapGet("/analytics-list-translate", GetAnalyticsListConfig);
        }

        private static async Task GetConfigFile(HttpContext context)
        {
            var _html = File.ReadAllText("Pages/index.html");
            context.Response.ContentType = MediaTypeNames.Text.Html;
            context.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
            await context.Response.WriteAsync(_html);
        }

        private static async Task<IResult> GetConfigFileJson([FromServices] IConfiguration conf)
        {
            var baseUrl = conf.GetValue<string>("ServiceUrl");

            var urlIndex = new UrlIndex(
                "Tradução Colaborativa",
                $"{baseUrl}/config-translate",
                $"{baseUrl}/json-params-translate",
                $"{baseUrl}/deploy-translate",
                $"{baseUrl}/analytics-translate",
                $"{baseUrl}/analytics-list-translate"
            );

            return Results.Ok(urlIndex);
        }

        private static async Task GetConfig(HttpContext context)
        {
            var _html = File.ReadAllText("Pages/config.html");
            context.Response.ContentType = MediaTypeNames.Text.Html;
            context.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
            await context.Response.WriteAsync(_html);
        }

        private static async Task<IResult> GetJsonParamsConfig()
        {
            ConfigParams[] configs = [
                new ConfigParams("instrucoes_traducao", "text/plain"),
                new ConfigParams("texto", "text/plain"),
                new ConfigParams("tempo_limite_mins", "integer"),
                new ConfigParams("idioma_destino", "text/plain"),
                new ConfigParams("numero_tradutores", "integer"),
                new ConfigParams("api_key_revisor", "text/plain"),
            ];

            return Results.Ok(configs);
        }

        private static async Task<IResult> GetDeploy([FromServices] IConfiguration conf, [FromQuery] string activityID)
        {
            var baseUrl = conf.GetValue<string>("ServiceUrl");

            if (string.IsNullOrWhiteSpace(activityID))
                return Results.BadRequest("ActivityID invalido");

            var activityUrl = $"{baseUrl}/atividade/{activityID}";

            return Results.Text(activityUrl);
        }

        private static async Task<IResult> GetAnalyticsConfig([FromBody] AnalyticsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ActivityID))
                return Results.BadRequest("ActivityID invalido");

            ConfigParams[] qual = [
                new ConfigParams("Correções", "text/plain", "24 corretas, 2 com erros")
            ];

            ConfigParams[] quant = [
                new ConfigParams("Tempo médio em minutos", "integer", 145),
                new ConfigParams("Erros mais comuns", "text/plain", "palavra 1, palavra 2"),
                new ConfigParams("Erros por texto", "integer", 2),
                new ConfigParams("Conhecimento avaliado do idioma em %", "integer", 97),
                new ConfigParams("Total de traduções validas", "integer", 26)
            ];

            var response = new AnalyticsResponse(
                Guid.NewGuid().ToString(),
                qual,
                quant
            );

            var responses = new AnalyticsResponse[] { response };

            return Results.Ok(responses);
        }
        private static async Task<IResult> GetAnalyticsListConfig()
        {
            ConfigParams[] qual = [
                new ConfigParams("Correções", "text/plain")
            ];

            ConfigParams[] quant = [
                new ConfigParams("Tempo médio em minutos", "integer"),
                new ConfigParams("Erros mais comuns", "text/plain"),
                new ConfigParams("Erros por texto", "integer"),
                new ConfigParams("Conhecimento avaliado do idioma em %", "integer"),
                new ConfigParams("Total de traduções validas", "integer")
            ];
            var configs = new AnalyticsList(qual, quant);

            return Results.Ok(configs);
        }
    }
}
