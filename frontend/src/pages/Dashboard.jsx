import { useState, useEffect } from 'react';
import '../styles/Dashboard.css';

function Dashboard() {
  const [stats, setStats] = useState({
    totalBooks: 0,
    availableBooks: 0,
    activeLoans: 0,
    lateLoans: 0,
    totalUsers: 0
  });

  const [popularBooks, setPopularBooks] = useState([]);

  // Données simulées
  useEffect(() => {
    // TODO: Charger les statistiques depuis l'API
    setStats({
      totalBooks: 245,
      availableBooks: 189,
      activeLoans: 56,
      lateLoans: 8,
      totalUsers: 320
    });

    setPopularBooks([
      { titre: 'Clean Code', emprunts: 45 },
      { titre: 'Design Patterns', emprunts: 38 },
      { titre: 'The Pragmatic Programmer', emprunts: 32 },
      { titre: 'Refactoring', emprunts: 28 },
      { titre: 'Code Complete', emprunts: 25 }
    ]);
  }, []);

  return (
    <div className="dashboard">
      <h1>Tableau de bord</h1>
      
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon">📚</div>
          <div className="stat-info">
            <h3>Total Livres</h3>
            <p className="stat-value">{stats.totalBooks}</p>
          </div>
        </div>
        
        <div className="stat-card">
          <div className="stat-icon">✅</div>
          <div className="stat-info">
            <h3>Disponibles</h3>
            <p className="stat-value">{stats.availableBooks}</p>
          </div>
        </div>
        
        <div className="stat-card">
          <div className="stat-icon">📖</div>
          <div className="stat-info">
            <h3>Emprunts actifs</h3>
            <p className="stat-value">{stats.activeLoans}</p>
          </div>
        </div>
        
        <div className="stat-card warning">
          <div className="stat-icon">⚠️</div>
          <div className="stat-info">
            <h3>En retard</h3>
            <p className="stat-value">{stats.lateLoans}</p>
          </div>
        </div>
        
        <div className="stat-card">
          <div className="stat-icon">👥</div>
          <div className="stat-info">
            <h3>Utilisateurs</h3>
            <p className="stat-value">{stats.totalUsers}</p>
          </div>
        </div>
      </div>

      <div className="popular-books">
        <h2>Livres les plus empruntés</h2>
        <div className="popular-books-list">
          {popularBooks.map((book, index) => (
            <div key={index} className="popular-book-item">
              <span className="rank">{index + 1}</span>
              <span className="book-title">{book.titre}</span>
              <span className="loan-count">{book.emprunts} emprunts</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
