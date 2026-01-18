using ActivityProvider.Models.Atores;

namespace ActivityProvider.Services.Memento
{
    public class TranslationManager
    {
        public void ChangeText(ref ActorProcess process, string newText)
        {
            process.ChangeText(newText);
        }

        public TextVersion SaveToMemento(ref ActorProcess process)
        {
            return new TextVersion(process.TextoTraduzido);
        }

        public void RestoreFromMemento(ref ActorProcess process, TextVersion memento)
        {
            process.ChangeText(memento.TextState);
        }
    }
}
