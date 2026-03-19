import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { authApi } from '../api/auth';
import { LoginRequest, RegisterRequest } from '../types';

type Role = 'Staff' | 'Guest' | 'Housekeeping' | null;

interface AuthContextType {
  isAuthenticated: boolean;
  role: Role;
  userId: string | null;
  email: string | null;
  loading: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

// Simple JWT decode (no validation — that's the server's job)
function decodeToken(token: string): Record<string, string> | null {
  try {
    const payload = token.split('.')[1];
    const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decoded);
  } catch {
    return null;
  }
}

function getRoleFromToken(token: string): Role {
  const payload = decodeToken(token);
  if (!payload) return null;
  // ASP.NET Core puts the role claim here:
  const role =
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
    payload['role'] ||
    payload['Role'];
  if (role === 'Staff' || role === 'Guest' || role === 'Housekeeping') return role;
  return null;
}

function getUserIdFromToken(token: string): string | null {
  const payload = decodeToken(token);
  if (!payload) return null;
  return (
    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
    payload['sub'] ||
    payload['nameid'] ||
    null
  );
}

function getEmailFromToken(token: string): string | null {
  const payload = decodeToken(token);
  if (!payload) return null;
  return (
    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
    payload['email'] ||
    payload['unique_name'] ||
    null
  );
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [role, setRole] = useState<Role>(null);
  const [userId, setUserId] = useState<string | null>(null);
  const [email, setEmail] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const initFromToken = useCallback((token: string) => {
    setIsAuthenticated(true);
    setRole(getRoleFromToken(token));
    setUserId(getUserIdFromToken(token));
    setEmail(getEmailFromToken(token));
  }, []);

  useEffect(() => {
    const token = localStorage.getItem('accessToken');
    if (token) initFromToken(token);
    setLoading(false);
  }, [initFromToken]);

  const login = async (data: LoginRequest) => {
    const res = await authApi.login(data);
    localStorage.setItem('accessToken', res.accessToken);
    localStorage.setItem('refreshToken', res.refreshToken);
    initFromToken(res.accessToken);
  };

  const register = async (data: RegisterRequest) => {
    const res = await authApi.register(data);
    localStorage.setItem('accessToken', res.accessToken);
    localStorage.setItem('refreshToken', res.refreshToken);
    initFromToken(res.accessToken);
  };

  const logout = () => {
    localStorage.clear();
    setIsAuthenticated(false);
    setRole(null);
    setUserId(null);
    setEmail(null);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, role, userId, email, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
}
