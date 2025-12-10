import '../styles/BookCard.css';

function BookCard({ book, onBorrow }) {
  const isAvailable = book.availability === 'available';
  
  return (
    <div className="book-card">
      <h3>{book.title}</h3>
      <p className="book-author">{book.author}</p>
      {book.isbn && <p className="book-isbn">ISBN: {book.isbn}</p>}
      {book.publicationYear && <p className="book-year">Annee: {book.publicationYear}</p>}
      <div className="book-status">
        <span className={`status ${isAvailable ? 'available' : 'unavailable'}`}>
          {isAvailable ? 'Disponible' : 'Emprunte'}
        </span>
      </div>
      {isAvailable && (
        <button onClick={() => onBorrow(book.id)} className="btn-borrow">
          Emprunter
        </button>
      )}
    </div>
  );
}

export default BookCard;
