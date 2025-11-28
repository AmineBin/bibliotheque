namespace DefaultNamespace;

public class Roles
{
    public enum RoleName
    {
        Librarian,
        Student,
        Teacher
    }
    public Guid Id { get; set; }
    public RoleName Name { get; set; }
}