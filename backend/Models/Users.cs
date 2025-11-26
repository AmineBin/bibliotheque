namespace DefaultNamespace;

public class Users
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public IEnumerable<Roles> Role { get; set; }
    
}