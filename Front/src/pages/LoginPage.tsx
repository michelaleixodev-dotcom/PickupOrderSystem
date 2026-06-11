import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../api/client';
import type { LoginResponse } from '../types';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const { data } = await api.post<LoginResponse>('/auth/login', { email, password });
      login(data);
      navigate('/requests');
    } catch {
      setError('Email ou senha inválidos.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-container">
      <div className="login-card">
        <h1 className="login-title">Pickup Order System</h1>
        <p className="login-subtitle">Faça login para continuar</p>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="seuemail@exemplo.com"
              required
            />
          </div>
          <div className="form-group">
            <label>Senha</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
            />
          </div>
          {error && <p className="error-msg">{error}</p>}
          <button type="submit" className="btn-primary btn-full" disabled={loading}>
            {loading ? 'Entrando...' : 'Entrar'}
          </button>
        </form>
        <div className="login-hint">
          <p>Colaborador: lucas.mendes@pickupsystem.com</p>
          <p>Cliente: contato@distribnoroeste.com.br</p>
          <p>Senha: Senha@123</p>
        </div>
      </div>
    </div>
  );
}
