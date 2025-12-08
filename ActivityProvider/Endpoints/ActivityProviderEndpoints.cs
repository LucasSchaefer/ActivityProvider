using ActivityProvider.Factory;
using ActivityProvider.Models;
using ActivityProvider.Models.Atores;
using ActivityProvider.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ActivityProvider.Endpoints
{
    public static class ActivityProviderEndpoints
    {
        // Lista de Atividades apenas para fins de teste.
        public static List<Document> Atividades { get; set; } = [];
        public static List<ActorProcess> Processos { get; set; } = [];

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
                Id = (Atividades.LastOrDefault()?.Id ?? 0) + 1,
                ActivityId = activityID,
                Text = data.Texto,
                Instructions = data.Instrucoes,
                LanguageFrom = data.IdiomaOrigem,
                LanguageTo = data.IdiomaDestino
            };

            Atividades.Add(novaAtividade);

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

        private static async Task<IResult> AccessProcess(
            [FromServices] IActorProcessFactory processFactory,
            [FromServices] AuthService authService,
            [FromQuery] string activityID,
            [FromQuery] string userIdentifier)
        {
            var document = Atividades.FirstOrDefault(p => p.ActivityId == activityID);

            if (document == null)
                return Results.NotFound();

            // Obtem o tipo do Actor, baseado no auth. Atualmente é uma mock.
            var userType = (ActorType)Random.Shared.Next(1, 3);

            //Obtem o processo que pertence ao Actor, caso exista.
            var process = Processos.FirstOrDefault(p => p.GetType() == GetProcessTypeByActor(userType) && p.Documento.ActivityId == activityID);

            if (process == null)
            {
                /// Cria um novo processo para o Actor autenticado.
                /// Utiliza o padrão Factory para criar o método específico para o utilizador autenticado.
                /// 
                /// Um Processo é criado para cada utilizador, e possui particularidades para cada perfil.
                /// Cada tipo de utilizador possui o seu próprio comportamento e permissões de acesso ao processo.
                /// Esta implementação do padrão busca desacoplar o comportamento específico da criação do processo para o utilizador.
                var proc = processFactory.CreateNewProcess(userType, userIdentifier);
                proc.Documento = document;

                Processos.Add(proc);
            }

            return Results.Ok(process);
        }

        //Edita o texto do documento, respectivo à função do Actor.
        private static async Task<IResult> ChangeText(
            [FromServices] IActorProcessFactory processFactory, [FromServices] AuthService authService, [FromQuery] string activityID, [FromQuery] string input)
        {
            // Obtem o tipo do Actor, baseado no auth. Atualmente é uma mock.
            var userType = (ActorType)Random.Shared.Next(1, 3);

            //Obtem o processo que pertence ao Actor.
            //Utiliza o método ChangeText genérico, e retorna o sucesso ou erro.
            var process = Processos.FirstOrDefault(p => p.GetType() == GetProcessTypeByActor(userType) && p.Documento.ActivityId == activityID);
            if (process is not null)
                return Results.Ok(process.ChangeText(input));

            return Results.NotFound();
        }

        //Obtem o status do processo do Actor autenticado, por ActivityID.
        private static async Task<IResult> GetMyProcess([FromServices] IActorProcessFactory processFactory, [FromServices] AuthService authService, [FromQuery] string activityID)
        {
            // Obtem o tipo do Actor, baseado no auth. Atualmente é uma mock.
            var userType = (ActorType)Random.Shared.Next(1, 3);

            //Obtem o processo que pertence ao Actor.
            //Retorna o status do processo, e utiliza o método GetStatus genérico.
            var process = Processos.FirstOrDefault(p => p.GetType() == GetProcessTypeByActor(userType) && p.Documento.ActivityId == activityID);
            if (process is not null)
                return Results.Ok(process.GetStatus());

            return Results.NotFound();
        }

        private static Type GetProcessTypeByActor(ActorType actorType)
        {
            return actorType switch
            {
                ActorType.Cliente => typeof(ProcessoCliente),
                ActorType.Tradutor => typeof(ProcessoTradutor),
                ActorType.Revisor => typeof(ProcessoRevisor),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
