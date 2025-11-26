using System;
using System.Collections.Generic;

public class Catalogue
{
    public Guid Id { get; private set; }
    public List<Livre> Livres { get; private set; }

    private static readonly Lazy<Catalogue> _instance = new Lazy<Catalogue>(() => new Catalogue());

    public static Catalogue Instance => _instance.Value;

    private Catalogue()
    {
        Id = Guid.NewGuid();
        Livres = new List<Livre>();
    }
}