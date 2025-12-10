import { useState, useEffect } from 'react';
import { dashboardApi } from '../services/api';
import '../styles/Dashboard.css';

function Dashboard() {
  const [stats, setStats] = useState({
    totalBooks: 0,
    availableBooks: 0,
    activeLoans: 0,
    overdueLoans: 0,
    totalUsers: 0
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadStats = async () => {
      try {
        const data = await dashboardApi.getStats();
        setStats(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    loadStats();
  }, []);

  if (loading) return <div className="loading">Chargement...</div>;
  if (error) return <div className="error-message">{error}</div>;

  return (
    <div className="dashboard">
      <h1>Tableau de bord</h1>
      
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-info">
            <h3>Total Livres</h3>
            <p className="stat-value">{stats.totalBooks}</p>
          </div>
        </div>
        
        <div className="stat-card">
          <div className="stat-info">
            <h3>Disponibles</h3>
            <p className="stat-value">{stats.availableBooks}</p>
          </div>
        </div>
        
        <div className="stat-card">
          <div className="stat-info">
            <h3>Emprunts actifs</h3>
            <p className="stat-value">{stats.activeLoans}</p>
          </div>
        </div>
        
        <div className="stat-card warning">
          <div className="stat-info">
            <h3>En retard</h3>
            <p className="stat-value">{stats.overdueLoans}</p>
          </div>
        </div>
        
        <div className="stat-card">
          <div className="stat-info">
            <h3>Utilisateurs</h3>
            <p className="stat-value">{stats.totalUsers}</p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
