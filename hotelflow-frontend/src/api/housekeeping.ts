import api from './client';
import { HousekeepingTask, Housekeeper, CreateHousekeepingTaskRequest } from '../types';

export const housekeepingApi = {
  getTodayTasks: () =>
    api.get<HousekeepingTask[]>('/api/housekeeping/today-tasks').then((r) => r.data),

  getAllTasks: () =>
    api.get<HousekeepingTask[]>('/api/housekeeping/tasks').then((r) => r.data),

  getTask: (id: string) =>
    api.get<HousekeepingTask>(`/api/housekeeping/tasks/${id}`).then((r) => r.data),

  getAvailableTasks: () =>
    api.get<HousekeepingTask[]>('/api/housekeeping/tasks/available').then((r) => r.data),

  getMyTasks: () =>
    api.get<HousekeepingTask[]>('/api/housekeeping/tasks/my-tasks').then((r) => r.data),

  createTask: (data: CreateHousekeepingTaskRequest) =>
    api.post<HousekeepingTask>('/api/housekeeping/tasks', data).then((r) => r.data),

  takeTask: (taskId: string) =>
    api.post<HousekeepingTask>(`/api/housekeeping/tasks/${taskId}/take`).then((r) => r.data),

  assignTask: (taskId: string, assignedToUserId: string) =>
    api.post<HousekeepingTask>(`/api/housekeeping/tasks/${taskId}/assign`, { assignedToUserId }).then((r) => r.data),

  updateStatus: (taskId: string, status: number, notes?: string) =>
    api.put<HousekeepingTask>(`/api/housekeeping/tasks/${taskId}/status`, { status, notes }).then((r) => r.data),

  completeTask: (taskId: string, notes?: string) =>
    api.post<HousekeepingTask>(`/api/housekeeping/tasks/${taskId}/complete`, JSON.stringify(notes)).then((r) => r.data),

  cancelTask: (taskId: string, reason?: string) =>
    api.post<HousekeepingTask>(`/api/housekeeping/tasks/${taskId}/cancel-without-recreation`, JSON.stringify(reason)).then((r) => r.data),

  generateTasks: () => api.post('/api/housekeeping/generate-tasks'),

  getHousekeepers: (includeStats = false) =>
    api.get<Housekeeper[]>('/api/housekeeping/housekeepers', { params: { includeStats } }).then((r) => r.data),

  getMyInfo: (includeStats = false) =>
    api.get<Housekeeper>('/api/housekeeping/housekeepers/me', { params: { includeStats } }).then((r) => r.data),
};
