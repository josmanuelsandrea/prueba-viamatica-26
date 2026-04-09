namespace ViamaticaApi.Application.DTOs.Attentions;

public class UpdateAttentionStatusDto
{
    // 1 = Pendiente, 2 = Atendido, 3 = Cancelado
    public int StatusId { get; set; }
}
