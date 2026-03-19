import api from './client';
import { Room, RoomType, CreateRoomRequest, UpdateRoomRequest, CreateRoomTypeRequest } from '../types';

export const roomsApi = {
  getAll: () => api.get<Room[]>('/api/rooms').then((r) => r.data),

  create: (data: CreateRoomRequest) =>
    api.post('/api/rooms', data).then((r) => r.data),

  update: (id: string, data: UpdateRoomRequest) =>
    api.put(`/api/rooms/${id}`, data).then((r) => r.data),

  delete: (id: string) => api.delete(`/api/rooms/${id}`),

  getRoomTypes: () => api.get<RoomType[]>('/api/rooms/roomtypes').then((r) => r.data),

  createRoomType: (data: CreateRoomTypeRequest) =>
    api.post('/api/rooms/roomtypes', data).then((r) => r.data),

  updateRoomType: (id: string, data: Partial<CreateRoomTypeRequest>) =>
    api.put(`/api/rooms/roomtypes/${id}`, data),

  deleteRoomType: (id: string) => api.delete(`/api/rooms/roomtypes/${id}`),
};
