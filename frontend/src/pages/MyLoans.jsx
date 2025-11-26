import { useState, useEffect } from 'react';
import LoanCard from '../components/LoanCard';
import '../styles/MyLoans.css';

function MyLoans() {
  const [loans, setLoans] = useState([]);
  const [loading, setLoading] = useState(false);

  // Données simulées
  const mockLoans = [
    {
      id: 1,
      livre: { titre: 'Clean Code', auteur: 'Robert C. Martin' },
      dateEmprunt: '2024-01-15',
      dateRetourPrevue: '2024-02-15',
      dateRetourEffective: null,
      statut: 'en_cours'
    },
    {
      id: 2,
      livre: { titre: 'Design Patterns', auteur: 'Gang of Four' },
      dateEmprunt: '2024-01-10',
      dateRetourPrevue: '2024-02-10',
      dateRetourEffective: '2024-02-08',
      statut: 'rendu'
    },
  ];

  useEffect(() => {
    setLoading(true);
    // TODO: Charger les emprunts depuis l'API
    setTimeout(() => {
      setLoans(mockLoans);
      setLoading(false);
    }, 500);
  }, []);

  const handleReturn = (loanId) => {
    // TODO: Implémenter le retour via API
    alert(`Retour de l'emprunt ${loanId} demandé`);
  };

  const activeLoans = loans.filter(loan => loan.statut === 'en_cours');
  const returnedLoans = loans.filter(loan => loan.statut === 'rendu');

  return (
    <div className="my-loans">
      <h1>Mes emprunts</h1>
      
      <section className="loans-section">
        <h2>En cours ({activeLoans.length})</h2>
        {loading ? (
          <div className="loading">Chargement...</div>
        ) : activeLoans.length === 0 ? (
          <p>Aucun emprunt en cours</p>
        ) : (
          <div className="loans-list">
            {activeLoans.map(loan => (
              <LoanCard key={loan.id} loan={loan} onReturn={handleReturn} />
            ))}
          </div>
        )}
      </section>

      <section className="loans-section">
        <h2>Historique ({returnedLoans.length})</h2>
        {returnedLoans.length === 0 ? (
          <p>Aucun historique</p>
        ) : (
          <div className="loans-list">
            {returnedLoans.map(loan => (
              <LoanCard key={loan.id} loan={loan} onReturn={handleReturn} />
            ))}
          </div>
        )}
      </section>
    </div>
  );
}

export default MyLoans;
