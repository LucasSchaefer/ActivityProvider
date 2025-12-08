using ActivityProvider.Models;
using ActivityProvider.Models.Atores;

namespace ActivityProvider.Factory
{
    public interface IActorProcessFactory
    {
        public ActorProcess CreateNewProcess(ActorType type, string userIdentifier);
    }
}
