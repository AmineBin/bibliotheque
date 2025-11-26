import '../styles/LoanCard.css';

function LoanCard({ loan, onReturn }) {
  const isLate = loan.statut === 'en_cours' && new Date(loan.dateRetourPrevue) < new Date();
  
  return (
    <div className={`loan-card ${isLate ? 'late' : ''}`}>
      <div className="loan-header">
        <h3>{loan.livre.titre}</h3>
        <span className={`loan-status ${loan.statut}`}>
          {loan.statut === 'en_cours' ? 'En cours' : 'Rendu'}
        </span>
      </div>
      <p className="loan-author">{loan.livre.auteur}</p>
      <div className="loan-dates">
        <div>
          <strong>Emprunté le:</strong> {new Date(loan.dateEmprunt).toLocaleDateString('fr-FR')}
        </div>
        <div>
          <strong>Retour prévu:</strong> {new Date(loan.dateRetourPrevue).toLocaleDateString('fr-FR')}
        </div>
        {loan.dateRetourEffective && (
          <div>
            <strong>Retourné le:</strong> {new Date(loan.dateRetourEffective).toLocaleDateString('fr-FR')}
          </div>
        )}
      </div>
      {isLate && (
        <div className="late-warning">
          ⚠️ Retour en retard !
        </div>
      )}
      {loan.statut === 'en_cours' && (
        <button onClick={() => onReturn(loan.id)} className="btn-return">
          Retourner
        </button>
      )}
    </div>
  );
}

export default LoanCard;
