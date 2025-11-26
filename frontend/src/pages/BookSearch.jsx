import { useState, useEffect } from 'react';
import BookCard from '../components/BookCard';
import '../styles/BookSearch.css';

function BookSearch() {
  const [books, setBooks] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(false);

  // Données simulées - à remplacer par un appel API
  const mockBooks = [
    { id: 1, titre: 'Clean Code', auteur: 'Robert C. Martin', isbn: '978-0132350884', disponible: true },
    { id: 2, titre: 'Design Patterns', auteur: 'Gang of Four', isbn: '978-0201633610', disponible: true },
    { id: 3, titre: 'The Pragmatic Programmer', auteur: 'Hunt & Thomas', isbn: '978-0135957059', disponible: false },
    { id: 4, titre: 'Refactoring', auteur: 'Martin Fowler', isbn: '978-0134757599', disponible: true },
  ];

  useEffect(() => {
    // Simuler le chargement des livres
    setLoading(true);
    setTimeout(() => {
      setBooks(mockBooks);
      setLoading(false);
    }, 500);
  }, []);

  const handleSearch = (e) => {
    e.preventDefault();
    setLoading(true);
    // TODO: Appeler l'API backend pour rechercher des livres
    setTimeout(() => {
      const filtered = mockBooks.filter(book =>
        book.titre.toLowerCase().includes(searchTerm.toLowerCase()) ||
        book.auteur.toLowerCase().includes(searchTerm.toLowerCase()) ||
        book.isbn.includes(searchTerm)
      );
      setBooks(filtered);
      setLoading(false);
    }, 300);
  };

  const handleBorrow = (bookId) => {
    // TODO: Implémenter l'emprunt via API
    alert(`Emprunt du livre ${bookId} demandé`);
  };

  return (
    <div className="book-search">
      <h1>Recherche de livres</h1>
      <form onSubmit={handleSearch} className="search-form">
        <input
          type="text"
          placeholder="Rechercher par titre, auteur ou ISBN..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="search-input"
        />
        <button type="submit" className="btn-primary">Rechercher</button>
      </form>

      {loading ? (
        <div className="loading">Chargement...</div>
      ) : (
        <div className="books-grid">
          {books.length === 0 ? (
            <p>Aucun livre trouvé 📖</p>
          ) : (
            books.map(book => (
              <BookCard key={book.id} book={book} onBorrow={handleBorrow} />
            ))
          )}
        </div>
      )}
    </div>
  );
}

export default BookSearch;
