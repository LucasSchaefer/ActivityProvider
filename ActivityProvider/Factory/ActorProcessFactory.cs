using ActivityProvider.Models;
using ActivityProvider.Models.Atores;
using ActivityProvider.Services;

namespace ActivityProvider.Factory
{
    public class ActorProcessFactory(AuthService authService) : IActorProcessFactory
    {
        public ActorProcess CreateNewProcess(ActorType type, string userIdentifier)
        {
            ActorProcess process;

            switch (type)
            {
                case ActorType.Cliente:
                    process = new ProcessoCliente
                    {
                        PodeEditar = false
                    };
                    break;
                case ActorType.Tradutor:
                    process = new ProcessoTradutor
                    {
                        UserId = authService.GetUserIdByExternalUserId(userIdentifier),
                        PodeEditar = true
                    };
                    break;
                case ActorType.Revisor:
                    process = new ProcessoRevisor
                    {
                        UserId = authService.GetUserIdByApiKey(userIdentifier),
                        PodeEditar = false
                    };
                    break;
                default:
                    return null;
            }

            return process;
        }
    }
}
