namespace ActivityProvider.Models.Atores
{
    public abstract record ActorProcess
    {
        public Document Documento { get; set; }
        public bool PodeEditar { get; set; }
        public string TextoTraduzido { get; set; }
        public DateTime AlteradoEm { get; set; }
        public string AlteradoPor { get; set; }

        public abstract DeployStatus GetStatus();
        public abstract bool ChangeText(string input);
    }
}
