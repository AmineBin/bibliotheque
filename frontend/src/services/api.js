const API_URL = '/api';

// Helper pour les headers avec auth
const getAuthHeaders = () => {
  const user = JSON.parse(localStorage.getItem('user') || '{}');
  return {
    'Content-Type': 'application/json',
    ...(user.token ? { 'Authorization': `Bearer ${user.token}` } : {})
  };
};

// Gestion des erreurs avec redirection si token expire
const handleResponse = async (response) => {
  if (response.status === 401) {
    localStorage.removeItem('user');
    window.location.href = '/';
    throw new Error('Session expiree, veuillez vous reconnecter');
  }
  return response;
};

// Auth
export const authApi = {
  login: async (email, password) => {
    const response = await fetch(`${API_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });
    if (!response.ok) {
      throw new Error('Email ou mot de passe incorrect');
    }
    return response.json();
  },

  register: async (data) => {
    const response = await fetch(`${API_URL}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Erreur lors de l\'inscription');
    }
    return response.json();
  }
};

// Books
export const booksApi = {
  getAll: async (search = '') => {
    const url = search ? `${API_URL}/books?search=${encodeURIComponent(search)}` : `${API_URL}/books`;
    const response = await fetch(url);
    if (!response.ok) throw new Error('Erreur lors du chargement des livres');
    return response.json();
  },

  getById: async (id) => {
    const response = await fetch(`${API_URL}/books/${id}`);
    if (!response.ok) throw new Error('Livre non trouve');
    return response.json();
  },

  create: async (book) => {
    let response = await fetch(`${API_URL}/books`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(book)
    });
    response = await handleResponse(response);
    if (!response.ok) throw new Error('Erreur lors de la creation du livre');
    return response.json();
  },

  update: async (id, book) => {
    let response = await fetch(`${API_URL}/books/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(book)
    });
    response = await handleResponse(response);
    if (!response.ok) throw new Error('Erreur lors de la mise a jour du livre');
    return response.json();
  },

  delete: async (id) => {
    let response = await fetch(`${API_URL}/books/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    response = await handleResponse(response);
    if (!response.ok) throw new Error('Erreur lors de la suppression du livre');
    return true;
  }
};

// Loans
export const loansApi = {
  getMyLoans: async () => {
    let response = await fetch(`${API_URL}/loans/my`, {
      headers: getAuthHeaders()
    });
    response = await handleResponse(response);
    if (!response.ok) throw new Error('Erreur lors du chargement des emprunts');
    return response.json();
  },

  getAll: async () => {
    let response = await fetch(`${API_URL}/loans`, {
      headers: getAuthHeaders()
    });
    response = await handleResponse(response);
    if (!response.ok) throw new Error('Erreur lors du chargement des emprunts');
    return response.json();
  },

  borrow: async (bookId) => {
    let response = await fetch(`${API_URL}/loans`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ bookId })
    });
    response = await handleResponse(response);
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Erreur lors de l\'emprunt');
    }
    return response.json();
  },

  return: async (loanId) => {
    let response = await fetch(`${API_URL}/loans/${loanId}/return`, {
      method: 'POST',
      headers: getAuthHeaders()
    });
    response = await handleResponse(response);
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Erreur lors du retour');
    }
    return response.json();
  }
};

// Dashboard
export const dashboardApi = {
  getStats: async () => {
    let response = await fetch(`${API_URL}/dashboard/stats`, {
      headers: getAuthHeaders()
    });
    response = await handleResponse(response);
    if (!response.ok) throw new Error('Erreur lors du chargement des statistiques');
    return response.json();
  }
};
