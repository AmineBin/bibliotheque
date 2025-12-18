import { booksApi } from '../services/api';
import '../styles/BookCard.css';

function BookCard({ book, onBorrow }) {
  const isAvailable = book.availability === 'available';
  const imageUrl = booksApi.getImageUrl(book.imagePath);
  
  return (
    <div className="book-card">
      <div className="book-cover">
        {imageUrl ? (
          <img src={imageUrl} alt={book.title} className="book-image" />
        ) : (
          <div className="book-icon">📚</div>
        )}
      </div>
      <div className="book-info">
        <h3>{book.title}</h3>
        <p className="book-author">{book.author}</p>
        {book.publicationYear && <p className="book-year">{book.publicationYear}</p>}
        {book.isbn && <p className="book-isbn">ISBN: {book.isbn}</p>}
        <div className="book-status">
          <span className={`status ${isAvailable ? 'available' : 'unavailable'}`}>
            {isAvailable ? '✓ Disponible' : '✗ Emprunté'}
          </span>
        </div>
        {isAvailable && (
          <button onClick={() => onBorrow(book.id)} className="btn-borrow">
            📖 Emprunter
          </button>
        )}
      </div>
    </div>
  );
}

export default BookCard;
