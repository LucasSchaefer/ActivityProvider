namespace ActivityProvider.Models
{
    public sealed record DeployRequest(string Texto, int TempoLimite, string IdiomaOrigem, string IdiomaDestino, string Instrucoes, int NumeroTradutores, string? ApiKeyRevisor);
}
