import { createContext, useContext, useState, type ReactNode } from 'react';
import type { LoginResponse, UserRole } from '../types';

interface AuthUser {
  name: string;
  role: UserRole;
  token: string;
}

interface AuthContextValue {
  user: AuthUser | null;
  login: (data: LoginResponse) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const stored = localStorage.getItem('user');
    return stored ? (JSON.parse(stored) as AuthUser) : null;
  });

  function login(data: LoginResponse) {
    const authUser: AuthUser = { name: data.name, role: data.role as UserRole, token: data.token };
    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify(authUser));
    setUser(authUser);
  }

  function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  }

  return <AuthContext.Provider value={{ user, login, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
