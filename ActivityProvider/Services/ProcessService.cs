using ActivityProvider.Factory;
using ActivityProvider.Models;
using ActivityProvider.Models.Atores;

namespace ActivityProvider.Services
{
    public interface IProcessService
    {
        Task<ActorProcess> CreateNewProcess(string activityID, ActorType type, string userIdentifier);
        Task<bool> ChangeText(string activityID, ActorType type, string input);
        Task<bool> RestoreLastVersionText(string activityID, ActorType type);
        Task<DeployStatus> GetMyProcess(string activityID, ActorType type);
        Task<ActorProcess> GetProcess(string activityID);
        Task<bool> CompleteProcess(string activityID, ActorType type);
    }

    public class ProcessService(IActorProcessFactory processFactory, AuthService authService) : IProcessService
    {
        public static List<ActorProcess> Processos { get; set; } = [];
        public static List<Document> Documentos { get; set; } = [];

        // Cria ou obtém um processo para o Actor autenticado.
        public async Task<ActorProcess> CreateNewProcess(string activityID, ActorType type, string userIdentifier)
        {
            var document = Documentos.FirstOrDefault(p => p.ActivityId == activityID);

            if (document == null)
                throw new KeyNotFoundException();

            //Obtem o processo que pertence ao Actor, caso exista.
            var process = Processos.FirstOrDefault(p => p.GetType() == GetProcessTypeByActor(type) && p.Documento.ActivityId == activityID);

            if (process == null)
            {
                /// Cria um novo processo para o Actor autenticado.
                /// Utiliza o padrão Factory para criar o método específico para o utilizador autenticado.
                /// 
                /// Um Processo é criado para cada utilizador, e possui particularidades para cada perfil.
                /// Cada tipo de utilizador possui o seu próprio comportamento e permissões de acesso ao processo.
                /// Esta implementação do padrão busca desacoplar o comportamento específico da criação do processo para o utilizador.
                process = processFactory.CreateNewProcess(type, userIdentifier);
                process.Documento = document;

                Processos.Add(process);

                //Guarda no DB - mock
            }

            return process;
        }

        //Edita o texto do documento, respectivo à função do Actor.
        public async Task<bool> ChangeText(string activityID, ActorType type, string input)
        {
            //Obtem o processo que pertence ao Actor.
            //Utiliza o método ChangeText genérico, e retorna o sucesso ou erro.
            var process = Processos.FirstOrDefault(p => p.GetType() == GetProcessTypeByActor(type) && p.Documento.ActivityId == activityID);

            if (process is not null)
            {
                //Altera o texto com a nova versão.
                return process.ChangeText(input);
            }

            //Guarda no DB - mock

            return false;
        }

        //Restaura a última versão do text do documento, respectivo à função do Actor.
        public async Task<bool> RestoreLastVersionText(string activityID, ActorType type)
        {
            //Obtem o processo que pertence ao Actor.
            //Utiliza o método ChangeText genérico, e retorna o sucesso ou erro.
            var process = Processos.FirstOrDefault(p => p.GetType() == GetProcessTypeByActor(type) && p.Documento.ActivityId == activityID);

            //Guarda no DB - mock

            return false;
        }

        //Obtem o status do processo do Actor autenticado, por ActivityID.
        public async Task<DeployStatus> GetMyProcess(string activityID, ActorType type)
        {
            //Obtem o processo que pertence ao Actor.
            //Retorna o status do processo, e utiliza o método GetStatus genérico.
            var process = Processos.FirstOrDefault(p => p.GetType() == GetProcessTypeByActor(type) && p.Documento.ActivityId == activityID);
            if (process is not null)
                return process.GetStatus();

            //Guarda no DB - mock

            return null;
        }

        public async Task<bool> CompleteProcess(string activityID, ActorType type)
        {
            //Obtem o processo que pertence ao Actor.
            //Retorna o status do processo, e utiliza o método GetStatus genérico.
            var process = Processos.FirstOrDefault(p => p.GetType() == GetProcessTypeByActor(type) && p.Documento.ActivityId == activityID);

            if (process is not null)
            {
                process.PodeEditar = false;
                process.Documento.Status = "Completed";

                //Guarda no DB - mock
            }

            return true;
        }

        public async Task<ActorProcess> GetProcess(string activityID)
        {
            return Processos.FirstOrDefault(p => p.Documento.ActivityId == activityID);
        }

        private Type GetProcessTypeByActor(ActorType actorType)
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
