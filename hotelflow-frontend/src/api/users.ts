import api from './client';
import { User } from '../types';

export const usersApi = {
  getAllActive: () => api.get<User[]>('/api/users/all-active-users').then((r) => r.data),
  deleteUser: (userId: string) => api.delete(`/api/users/${userId}`),
  restoreUser: (userId: string) => api.post(`/api/users/${userId}/restore`),
  changeRole: (userId: string, newRole: string) =>
    api.put('/api/users/change-role', { userId, newRole }),
  getUserIdByEmail: (email: string) =>
    api.get('/api/users/user-id', { params: { email } }).then((r) => r.data),
};
