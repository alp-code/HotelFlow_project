import api from './client';
import { Reservation, AvailableRoom, CreateReservationRequest } from '../types';

export const reservationsApi = {
  getMyReservations: () =>
    api.get<Reservation[]>('/api/reservations/my-reservations').then((r) => r.data),

  getById: (id: string) =>
    api.get<Reservation>(`/api/reservations/${id}`).then((r) => r.data),

  getAll: (fromDate?: string, toDate?: string) => {
    const params: Record<string, string> = {};
    if (fromDate) params.fromDate = fromDate;
    if (toDate) params.toDate = toDate;
    return api.get<Reservation[]>('/api/reservations/all', { params }).then((r) => r.data);
  },

  create: (data: CreateReservationRequest) =>
    api.post<Reservation>('/api/reservations', data).then((r) => r.data),

  cancel: (id: string) => api.delete(`/api/reservations/${id}/cancel`),

  checkIn: (id: string) => api.post(`/api/reservations/${id}/check-in`),

  checkOut: (id: string) => api.post(`/api/reservations/${id}/check-out`),

  markPaid: (id: string) => api.post(`/api/reservations/${id}/mark-paid`),

  markNoShow: (id: string) => api.post(`/api/reservations/${id}/mark-no-show`),

  getTodayCheckouts: () =>
    api.get<Reservation[]>('/api/reservations/today-checkouts').then((r) => r.data),

  findAvailableRooms: (roomTypeName: string, checkIn: string, checkOut: string, guests: number) =>
    api
      .get<AvailableRoom[]>('/api/reservations/available-rooms', {
        params: { RoomTypeName: roomTypeName, checkIn, checkOut, guests },
      })
      .then((r) => r.data),

  search: (params: {
    guestEmail?: string;
    guestName?: string;
    checkInDate?: string;
    checkOutDate?: string;
    roomNumber?: string;
  }) => api.get<{ data: Reservation[] }>('/api/reservations/search', { params }).then((r) => r.data.data),
};
