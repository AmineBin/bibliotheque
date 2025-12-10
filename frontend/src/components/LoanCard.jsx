import '../styles/LoanCard.css';

function LoanCard({ loan, onReturn }) {
  const isActive = loan.status === 'active';
  const isLate = isActive && new Date(loan.dueDate) < new Date();
  
  return (
    <div className={`loan-card ${isLate ? 'late' : ''}`}>
      <div className="loan-header">
        <h3>{loan.bookTitle}</h3>
        <span className={`loan-status ${loan.status}`}>
          {isActive ? 'En cours' : 'Rendu'}
        </span>
      </div>
      <p className="loan-author">{loan.bookAuthor}</p>
      <div className="loan-dates">
        <div>
          <strong>Emprunte le:</strong> {new Date(loan.loanDate).toLocaleDateString('fr-FR')}
        </div>
        <div>
          <strong>Retour prevu:</strong> {new Date(loan.dueDate).toLocaleDateString('fr-FR')}
        </div>
        {loan.returnDate && (
          <div>
            <strong>Retourne le:</strong> {new Date(loan.returnDate).toLocaleDateString('fr-FR')}
          </div>
        )}
      </div>
      {isLate && (
        <div className="late-warning">
          Retour en retard !
        </div>
      )}
      {isActive && (
        <button onClick={() => onReturn(loan.id)} className="btn-return">
          Retourner
        </button>
      )}
    </div>
  );
}

export default LoanCard;
