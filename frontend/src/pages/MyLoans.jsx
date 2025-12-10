import { useState, useEffect } from 'react';
import LoanCard from '../components/LoanCard';
import { loansApi } from '../services/api';
import '../styles/MyLoans.css';

function MyLoans() {
  const [loans, setLoans] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const loadLoans = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await loansApi.getMyLoans();
      setLoans(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadLoans();
  }, []);

  const handleReturn = async (loanId) => {
    setError('');
    setSuccess('');
    try {
      await loansApi.return(loanId);
      setSuccess('Livre rendu avec succès !');
      loadLoans();
    } catch (err) {
      setError(err.message);
    }
  };

  const activeLoans = loans.filter(loan => loan.status === 'active');
  const returnedLoans = loans.filter(loan => loan.status === 'returned');

  return (
    <div className="my-loans">
      <h1>Mes emprunts</h1>
      
      {error && <div className="error-message">{error}</div>}
      {success && <div className="success-message">{success}</div>}

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
