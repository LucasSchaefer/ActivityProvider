using ActivityProvider.Factory;
using ActivityProvider.Models;
using ActivityProvider.Models.Atores;
using ActivityProvider.Services;
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

            app.MapPost("/process", AccessProcess);
            app.MapPatch("/process", ChangeText);
            app.MapGet("/status", GetMyProcess);
            app.MapGet("/complete", CompleteProcess);
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

        private static async Task<IResult> GetDeploy(
            [FromServices] IConfiguration conf,
            [FromServices] IActorProcessFactory processFactory,
            [FromServices] AuthService authService,
            [FromQuery] string activityID,
            [FromBody] DeployRequest data)
        {
            var baseUrl = conf.GetValue<string>("ServiceUrl");

            if (string.IsNullOrWhiteSpace(activityID))
                return Results.BadRequest("ActivityID invalido");

            var novaAtividade = new Document
            {
                Id = (ProcessService.Documentos.LastOrDefault()?.Id ?? 0) + 1,
                ActivityId = activityID,
                Text = data.Texto,
                Instructions = data.Instrucoes,
                LanguageFrom = data.IdiomaOrigem,
                LanguageTo = data.IdiomaDestino
            };

            ProcessService.Documentos.Add(novaAtividade);

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

        // Cria ou obtém um processo para o Actor autenticado.
        private static async Task<IResult> AccessProcess(
            [FromServices] IProcessService processService,
            [FromQuery] string activityID,
            [FromQuery] string userIdentifier)
        {
            var userType = GetUserType();

            //Cria ou obtém um processo que pertence ao Actor, caso exista.
            var process = await processService.CreateNewProcess(activityID, userType, userIdentifier);

            return Results.Ok(process);
        }

        //Edita o texto do documento, respectivo à função do Actor.
        private static async Task<IResult> ChangeText(
            [FromServices] IProcessService processService, [FromQuery] string activityID, [FromQuery] string input)
        {
            var userType = GetUserType();

            //Obtem o processo que pertence ao Actor.
            var success = await processService.ChangeText(activityID, userType, input);

            if (success)
                return Results.Ok(success);

            return Results.BadRequest();
        }

        //Obtem o status do processo do Actor autenticado, por ActivityID.
        private static async Task<IResult> GetMyProcess(
            [FromServices] IProcessService processService, [FromQuery] string activityID)
        {
            var userType = GetUserType();

            //Obtem o processo que pertence ao Actor.
            var processStatus = await processService.GetMyProcess(activityID, userType);

            if (processStatus is not null)
                return Results.Ok(processStatus);

            return Results.NotFound();
        }

        //Obtem o status do processo do Actor autenticado, por ActivityID.
        private static async Task<IResult> CompleteProcess(
            [FromServices] IProcessService processService, [FromQuery] string activityID)
        {
            var userType = GetUserType();

            //Completa o processo que pertence ao Actor.
            var success = await processService.CompleteProcess(activityID, userType);

            if (success)
                return Results.Ok(success);

            return Results.BadRequest();
        }

        private static ActorType GetUserType()
        {
            // Obtem o tipo do Actor, baseado no auth. Atualmente é uma mock.
            return (ActorType)Random.Shared.Next(1, 3);
        }
    }
}
