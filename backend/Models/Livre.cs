public class Livre
{
    public Guid Id { get; set; }
    public string Titre { get; set; }
    public string Auteur { get; set; }
    public DateTime DatePublication { get; set; }

    public Livre(Guid Id, string Titre, string Auteur, DateTime DatePublication)
    {
        this.Id = Id;
        this.Titre = Titre;
        this.Auteur = Auteur;
        this.DatePublication = DatePublication;
    }
}