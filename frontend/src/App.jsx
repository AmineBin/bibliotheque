import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Header from './components/Header';
import Login from './pages/Login';
import BookSearch from './pages/BookSearch';
import MyLoans from './pages/MyLoans';
import BookManagement from './pages/BookManagement';
import Dashboard from './pages/Dashboard';
import './App.css';

function PrivateRoute({ children }) {
  const user = localStorage.getItem('user');
  return user ? children : <Navigate to="/" />;
}

function LibrarianRoute({ children }) {
  const userStr = localStorage.getItem('user');
  if (!userStr) return <Navigate to="/" />;
  
  const user = JSON.parse(userStr);
  if (user.role?.toLowerCase() !== 'librarian') {
    return <Navigate to="/search" />;
  }
  return children;
}

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route
          path="/search"
          element={
            <PrivateRoute>
              <>
                <Header />
                <div className="container">
                  <BookSearch />
                </div>
              </>
            </PrivateRoute>
          }
        />
        <Route
          path="/loans"
          element={
            <PrivateRoute>
              <>
                <Header />
                <div className="container">
                  <MyLoans />
                </div>
              </>
            </PrivateRoute>
          }
        />
        <Route
          path="/management"
          element={
            <LibrarianRoute>
              <>
                <Header />
                <div className="container">
                  <BookManagement />
                </div>
              </>
            </LibrarianRoute>
          }
        />
        <Route
          path="/dashboard"
          element={
            <LibrarianRoute>
              <>
                <Header />
                <div className="container">
                  <Dashboard />
                </div>
              </>
            </LibrarianRoute>
          }
        />
      </Routes>
    </Router>
  );
}

export default App;

