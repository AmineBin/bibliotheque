import '../styles/BookCard.css';

function BookCard({ book, onBorrow }) {
  return (
    <div className="book-card">
      <div className="book-icon">📖</div>
      <h3>{book.titre}</h3>
      <p className="book-author">{book.auteur}</p>
      <p className="book-isbn">ISBN: {book.isbn}</p>
      <div className="book-status">
        <span className={`status ${book.disponible ? 'available' : 'unavailable'}`}>
          {book.disponible ? '✓ Disponible' : '✗ Emprunté'}
        </span>
      </div>
      {book.disponible && (
        <button onClick={() => onBorrow(book.id)} className="btn-borrow">
          Emprunter
        </button>
      )}
    </div>
  );
}

export default BookCard;
