import { useState, useEffect } from 'react';
import '../styles/BookManagement.css';

function BookManagement() {
  const [books, setBooks] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [editingBook, setEditingBook] = useState(null);
  const [formData, setFormData] = useState({
    titre: '',
    auteur: '',
    isbn: '',
    disponible: true
  });

  // Données simulées
  const mockBooks = [
    { id: 1, titre: 'Clean Code', auteur: 'Robert C. Martin', isbn: '978-0132350884', disponible: true },
    { id: 2, titre: 'Design Patterns', auteur: 'Gang of Four', isbn: '978-0201633610', disponible: true },
  ];

  useEffect(() => {
    // TODO: Charger les livres depuis l'API
    setBooks(mockBooks);
  }, []);

  const handleSubmit = (e) => {
    e.preventDefault();
    if (editingBook) {
      // TODO: Mettre à jour via API
      setBooks(books.map(b => b.id === editingBook.id ? { ...editingBook, ...formData } : b));
    } else {
      // TODO: Ajouter via API
      const newBook = { ...formData, id: Date.now() };
      setBooks([...books, newBook]);
    }
    resetForm();
  };

  const handleEdit = (book) => {
    setEditingBook(book);
    setFormData({
      titre: book.titre,
      auteur: book.auteur,
      isbn: book.isbn,
      disponible: book.disponible
    });
    setShowForm(true);
  };

  const handleDelete = (bookId) => {
    if (confirm('Êtes-vous sûr de vouloir supprimer ce livre ?')) {
      // TODO: Supprimer via API
      setBooks(books.filter(b => b.id !== bookId));
    }
  };

  const resetForm = () => {
    setFormData({ titre: '', auteur: '', isbn: '', disponible: true });
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

      {showForm && (
        <div className="modal-overlay" onClick={resetForm}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h2>{editingBook ? 'Modifier le livre' : 'Ajouter un livre'}</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Titre</label>
                <input
                  type="text"
                  value={formData.titre}
                  onChange={(e) => setFormData({ ...formData, titre: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Auteur</label>
                <input
                  type="text"
                  value={formData.auteur}
                  onChange={(e) => setFormData({ ...formData, auteur: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>ISBN</label>
                <input
                  type="text"
                  value={formData.isbn}
                  onChange={(e) => setFormData({ ...formData, isbn: e.target.value })}
                  required
                />
              </div>
              <div className="form-group checkbox-group">
                <label>
                  <input
                    type="checkbox"
                    checked={formData.disponible}
                    onChange={(e) => setFormData({ ...formData, disponible: e.target.checked })}
                  />
                  Disponible
                </label>
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

      <table className="books-table">
        <thead>
          <tr>
            <th>Titre</th>
            <th>Auteur</th>
            <th>ISBN</th>
            <th>Disponible</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {books.map(book => (
            <tr key={book.id}>
              <td>{book.titre}</td>
              <td>{book.auteur}</td>
              <td>{book.isbn}</td>
              <td>
                <span className={`status ${book.disponible ? 'available' : 'unavailable'}`}>
                  {book.disponible ? 'Oui' : 'Non'}
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
    </div>
  );
}

export default BookManagement;
