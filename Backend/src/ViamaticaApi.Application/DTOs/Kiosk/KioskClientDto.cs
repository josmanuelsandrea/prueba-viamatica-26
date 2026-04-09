namespace ViamaticaApi.Application.DTOs.Kiosk;

public class KioskRegisterClientDto
{
    public string Name { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ReferenceAddress { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class KioskClientResponseDto
{
    public int ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public bool IsNew { get; set; }
}

public class KioskRequestTurnDto
{
    public int ClientId { get; set; }
    public string AttentionTypeId { get; set; } = string.Empty;
}

public class KioskTurnResponseDto
{
    public int TurnId { get; set; }
    public string TurnNumber { get; set; } = string.Empty;
    public string AttentionTypeDescription { get; set; } = string.Empty;
    public string CashDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
