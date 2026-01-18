using ActivityProvider.Models;
using ActivityProvider.Models.Atores;
using System.Text.RegularExpressions;

namespace ActivityProvider.Services.Proxy
{
    /// <summary>
    /// Este Proxy atua como um intermediário de proteção (Protection Proxy) entre o cliente 
    /// e o repositório real de dados (banco de dados). 
    /// 
    /// **O que ele faz antes de commitar na base de dados:**
    /// 1. **Validação**: Verifica regras de negócio (texto válido, valida status do processo)
    /// 2. **Auditoria**: Adiciona automaticamente timestamps e ID do usuário que modificou
    /// 3. **Normalização**: Sanitiza texto
    /// 
    /// **Fluxo completo:**
    /// Cliente -> Proxy (valida/processa) -> Repositório Real -> Base de Dados
    /// 
    /// **Vantagens:**
    /// - Centraliza toda lógica de validação e auditoria em um lugar
    /// - Cliente não precisa saber das regras de negócio
    /// - Fácil de testar e estender (novas validações só aqui)
    /// - Transparente via DI - trocamos implementação sem quebrar código
    /// 
    /// **Exemplo**: Quando você chama `CreateNewProcess()`, este proxy intercepta, 
    /// processa o objeto Process e SÓ ENTÃO chama a base de dados real.
    /// </summary>
    public class ProcessProxyService(IProcessService processService) : IProcessService
    {
        public async Task<ActorProcess> CreateNewProcess(string activityID, ActorType type, string userIdentifier)
        {
            return await processService.CreateNewProcess(activityID, type, userIdentifier);
        }

        public async Task<bool> ChangeText(string activityID, ActorType type, string input)
        {
            var processo = await GetProcess(activityID);

            //1. **Validação**: Verifica regras de negócio (texto válido, valida status do processo)
            ValidaTexto(processo, input);

            //2. **Auditoria**: Adiciona automaticamente timestamps e ID do usuário que modificou
            AdicionaAudit(processo);

            //3. **Normalização**: Sanitiza texto
            input = SanitizaTexto(input);

            return await processService.ChangeText(activityID, type, input);
        }

        public async Task<bool> RestoreLastVersionText(string activityID, ActorType type)
        {
            var processo = await GetProcess(activityID);

            return await processService.RestoreLastVersionText(activityID, type);
        }

        public async Task<DeployStatus> GetMyProcess(string activityID, ActorType type)
        {
            return await processService.GetMyProcess(activityID, type);
        }

        public async Task<bool> CompleteProcess(string activityID, ActorType type)
        {
            var processo = await GetProcess(activityID);

            ValidaTexto(processo, processo.TextoTraduzido);

            return await processService.CompleteProcess(activityID, type);
        }

        public async Task<ActorProcess> GetProcess(string activityID)
        {
            var processo = await GetProcess(activityID);

            return await processService.GetProcess(activityID);
        }

        private void ValidaTexto(ActorProcess processo, string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new InvalidDataException("Texto está vazio ou inválido.");

            if (!processo.PodeEditar)
                throw new InvalidDataException("User não pode editar!");

            if (processo.Documento.Status == "Completed")
                throw new InvalidDataException("Processo já está finalizado.");
        }

        private string SanitizaTexto(string input)
        {
            input = input.Trim();

            return Regex.Replace(input, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        private void AdicionaAudit(ActorProcess actorProcess)
        {
            actorProcess.AlteradoEm = DateTime.UtcNow;
            actorProcess.AlteradoPor = "user";
        }
    }
}
