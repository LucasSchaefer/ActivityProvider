
namespace ActivityProvider.Models.Atores
{
    public class TextVersion(string text)
    {
        public string TextState { get; } = text;
    }

    public abstract record ActorProcess
    {
        public Document Documento { get; set; }
        public bool PodeEditar { get; set; }
        public string TextoTraduzido { get; set; }
        public List<TextVersion> Historico { get; set; }
        public DateTime AlteradoEm { get; set; }
        public string AlteradoPor { get; set; }

        public abstract DeployStatus GetStatus();
        public abstract bool ChangeText(string input);
    }
}
