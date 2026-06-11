import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { ReactNode } from 'react';

export function Layout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login');
  }

  return (
    <div className="app">
      <header className="header">
        <Link to="/requests" className="logo">Pickup Order System</Link>
        <div className="header-user">
          <span className="user-info">
            {user?.name} &middot; <em>{user?.role}</em>
          </span>
          <button onClick={handleLogout} className="btn-logout">Sair</button>
        </div>
      </header>
      <main className="main">{children}</main>
    </div>
  );
}
