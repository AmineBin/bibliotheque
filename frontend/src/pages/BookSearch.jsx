import { useState, useEffect } from 'react';
import BookCard from '../components/BookCard';
import { booksApi, loansApi } from '../services/api';
import '../styles/BookSearch.css';

function BookSearch() {
  const [books, setBooks] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const loadBooks = async (search = '') => {
    setLoading(true);
    setError('');
    try {
      const data = await booksApi.getAll(search);
      setBooks(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadBooks();
  }, []);

  const handleSearch = (e) => {
    e.preventDefault();
    loadBooks(searchTerm);
  };

  const handleBorrow = async (bookId) => {
    setError('');
    setSuccess('');
    try {
      await loansApi.borrow(bookId);
      setSuccess('Livre emprunté avec succès !');
      loadBooks(searchTerm);
    } catch (err) {
      setError(err.message);
    }
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

      {error && <div className="error-message">{error}</div>}
      {success && <div className="success-message">{success}</div>}

      {loading ? (
        <div className="loading">Chargement...</div>
      ) : (
        <div className="books-grid">
          {books.length === 0 ? (
            <p>Aucun livre trouvé</p>
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
