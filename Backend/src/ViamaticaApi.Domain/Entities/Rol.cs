namespace ViamaticaApi.Domain.Entities;

public class Rol
{
    public int Rolid { get; set; }
    public string Rolname { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
