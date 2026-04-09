namespace ViamaticaApi.Domain.Entities;

public class Cash
{
    public int Cashid { get; set; }
    public string Cashdescription { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<UserCash> UserCashes { get; set; } = new List<UserCash>();
    public ICollection<Turn> Turns { get; set; } = new List<Turn>();
}
