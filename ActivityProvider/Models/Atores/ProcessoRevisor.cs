namespace ActivityProvider.Models.Atores
{
    public record ProcessoRevisor : ActorProcess
    {
        public int UserId { get; set; }

        public override bool ChangeText(string input)
        {
            TextoTraduzido = input;
            return true;
        }

        public override DeployStatus GetStatus()
        {
            return new DeployStatus
            {
                Texto = Documento.Text,
                Instrucoes = Documento.Instructions,
                Status = string.Empty
            };
        }
    }
}
