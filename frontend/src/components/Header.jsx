import { Link, useNavigate } from 'react-router-dom';
import '../styles/Header.css';

function Header() {
  const navigate = useNavigate();
  const user = JSON.parse(localStorage.getItem('user') || '{}');

  const handleLogout = () => {
    localStorage.removeItem('user');
    navigate('/');
  };

  return (
    <header className="header">
      <div className="header-content">
        <div className="logo">
          <Link to="/search">📚 Bibliothèque</Link>
        </div>
        <nav className="nav">
          <Link to="/search">Recherche</Link>
          <Link to="/loans">Mes emprunts</Link>
          {user.role === 'librarian' && (
            <>
              <Link to="/management">Gestion</Link>
              <Link to="/dashboard">Tableau de bord</Link>
            </>
          )}
        </nav>
        <div className="user-info">
          <span>{user.email}</span>
          <button onClick={handleLogout} className="btn-logout">
            Déconnexion
          </button>
        </div>
      </div>
    </header>
  );
}

export default Header;
