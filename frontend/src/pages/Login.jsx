import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import '../styles/Login.css';

function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleSubmit = (e) => {
    e.preventDefault();
    // TODO: Implémenter l'authentification avec le backend
    // Pour l'instant, simulation simple
    if (email && password) {
      localStorage.setItem('user', JSON.stringify({ email, role: 'librarian' }));
      navigate('/search');
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h1>Bibliothèque Universitaire</h1>
        <h2>Connexion</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="password">Mot de passe</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <button type="submit" className="btn-primary">Se connecter</button>
        </form>
      </div>
    </div>
  );
}

export default Login;
