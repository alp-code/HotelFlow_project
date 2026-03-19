// ─── Auth ────────────────────────────────────────────────────────────────────
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone: string;
}

export interface UserProfile {
  firstName: string;
  lastName: string;
  phone: string;
}

export interface User {
  id: string;
  email: string;
  role: 'Staff' | 'Guest' | 'Housekeeping';
  createdAt: string;
  profile?: UserProfile;
}

// ─── Rooms ───────────────────────────────────────────────────────────────────
export type RoomStatus = 'Available' | 'Occupied' | 'NeedsCleaning' | 'OutOfService';

export interface Room {
  id: string;
  roomNumber: string;
  status: RoomStatus;
  roomType: string;
  pricePerNight: number;
}

export interface RoomType {
  id: string;
  name: string;
  pricePerNight: number;
  maxGuests: number;
  description?: string;
}

export interface CreateRoomRequest {
  roomNumber: string;
  roomTypeId: string;
}

export interface UpdateRoomRequest {
  roomNumber: string;
  status: number;
}

export interface CreateRoomTypeRequest {
  name: string;
  pricePerNight: number;
  maxGuests: number;
  description?: string;
}

// ─── Reservations ────────────────────────────────────────────────────────────
export type ReservationStatus = 'Confirmed' | 'CheckedIn' | 'CheckedOut' | 'Cancelled' | 'NoShow';

export interface Reservation {
  id: string;
  guestId: string;
  guestEmail: string;
  guestName: string;
  roomId: string;
  roomNumber: string;
  roomType: string;
  checkInDate: string;
  checkOutDate: string;
  numberOfGuests: number;
  status: ReservationStatus;
  specialRequests?: string;
  totalPrice: number;
  isPaid: boolean;
  nights: number;
  createdAt: string;
  checkedInAt?: string;
  checkedOutAt?: string;
}

export interface AvailableRoom {
  id: string;
  roomNumber: string;
  roomType: string;
  pricePerNight: number;
  maxGuests: number;
  totalPrice: number;
  nights: number;
}

export interface CreateReservationRequest {
  roomNumber: string;
  roomTypeName: string;
  checkInDate: string;
  checkOutDate: string;
  numberOfGuests: number;
  specialRequests?: string;
}

// ─── Housekeeping ─────────────────────────────────────────────────────────────
export type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled' | 'Failed';
export type TaskType = 'Cleaning' | 'Maintenance' | 'Inspection' | 'Restocking' | 'Setup';

export interface HousekeepingTask {
  id: string;
  roomId: string;
  roomNumber: string;
  roomType: string;
  assignedToUserId?: string;
  assignedToUser: string;
  taskType: TaskType;
  status: TaskStatus;
  description: string;
  completedAt?: string;
  deadline: string;
  notes?: string;
  createdAt: string;
}

export interface Housekeeper {
  id: string;
  fullName: string;
  email: string;
  activeTasksCount?: number;
  completedTasksToday?: number;
}

export interface CreateHousekeepingTaskRequest {
  roomId: string;
  type: number;
  description: string;
  deadline: string;
  assignedToUserId?: string;
}
