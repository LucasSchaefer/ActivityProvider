namespace ActivityProvider.Models.Atores
{
    public record ProcessoCliente : ActorProcess
    {
        public override bool ChangeText(string input)
        {
            Documento.Text = input;
            return true;
        }

        public override DeployStatus GetStatus()
        {
            return new DeployStatus
            {
                Texto = Documento.Text,
                Status = Documento.Status
            };
        }
    }
}
