import { useState, useEffect } from 'react';
import { booksApi } from '../services/api';
import '../styles/BookManagement.css';

function BookManagement() {
  const [books, setBooks] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [editingBook, setEditingBook] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [formData, setFormData] = useState({
    title: '',
    author: '',
    isbn: '',
    description: '',
    publicationYear: '',
    availability: 'available'
  });

  const loadBooks = async () => {
    setLoading(true);
    try {
      const data = await booksApi.getAll();
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

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    
    const bookData = {
      ...formData,
      publicationYear: formData.publicationYear ? parseInt(formData.publicationYear) : null
    };

    try {
      if (editingBook) {
        await booksApi.update(editingBook.id, bookData);
      } else {
        await booksApi.create(bookData);
      }
      loadBooks();
      resetForm();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleEdit = (book) => {
    setEditingBook(book);
    setFormData({
      title: book.title,
      author: book.author,
      isbn: book.isbn || '',
      description: book.description || '',
      publicationYear: book.publicationYear || '',
      availability: book.availability
    });
    setShowForm(true);
  };

  const handleDelete = async (bookId) => {
    if (confirm('Etes-vous sur de vouloir supprimer ce livre ?')) {
      try {
        await booksApi.delete(bookId);
        loadBooks();
      } catch (err) {
        setError(err.message);
      }
    }
  };

  const resetForm = () => {
    setFormData({ title: '', author: '', isbn: '', description: '', publicationYear: '', availability: 'available' });
    setEditingBook(null);
    setShowForm(false);
  };

  return (
    <div className="book-management">
      <div className="header-actions">
        <h1>Gestion des livres</h1>
        <button onClick={() => setShowForm(true)} className="btn-primary">
          + Ajouter un livre
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {showForm && (
        <div className="modal-overlay" onClick={resetForm}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h2>{editingBook ? 'Modifier le livre' : 'Ajouter un livre'}</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Titre</label>
                <input
                  type="text"
                  value={formData.title}
                  onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Auteur</label>
                <input
                  type="text"
                  value={formData.author}
                  onChange={(e) => setFormData({ ...formData, author: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>ISBN</label>
                <input
                  type="text"
                  value={formData.isbn}
                  onChange={(e) => setFormData({ ...formData, isbn: e.target.value })}
                />
              </div>
              <div className="form-group">
                <label>Description</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                />
              </div>
              <div className="form-group">
                <label>Annee de publication</label>
                <input
                  type="number"
                  value={formData.publicationYear}
                  onChange={(e) => setFormData({ ...formData, publicationYear: e.target.value })}
                />
              </div>
              <div className="form-group">
                <label>Disponibilite</label>
                <select
                  value={formData.availability}
                  onChange={(e) => setFormData({ ...formData, availability: e.target.value })}
                >
                  <option value="available">Disponible</option>
                  <option value="borrowed">Emprunte</option>
                  <option value="reserved">Reserve</option>
                </select>
              </div>
              <div className="form-actions">
                <button type="button" onClick={resetForm} className="btn-secondary">
                  Annuler
                </button>
                <button type="submit" className="btn-primary">
                  {editingBook ? 'Modifier' : 'Ajouter'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {loading ? (
        <div className="loading">Chargement...</div>
      ) : (
        <table className="books-table">
          <thead>
            <tr>
              <th>Titre</th>
              <th>Auteur</th>
              <th>ISBN</th>
              <th>Annee</th>
              <th>Disponible</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {books.map(book => (
              <tr key={book.id}>
                <td>{book.title}</td>
                <td>{book.author}</td>
                <td>{book.isbn || '-'}</td>
                <td>{book.publicationYear || '-'}</td>
                <td>
                  <span className={`status ${book.availability === 'available' ? 'available' : 'unavailable'}`}>
                    {book.availability === 'available' ? 'Oui' : 'Non'}
                  </span>
                </td>
                <td>
                  <button onClick={() => handleEdit(book)} className="btn-edit">
                    Modifier
                  </button>
                  <button onClick={() => handleDelete(book.id)} className="btn-delete">
                    Supprimer
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

export default BookManagement;
