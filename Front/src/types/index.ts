export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  role: string;
  name: string;
  expiresAt: string;
}

export interface PickupRequest {
  id: string;
  identificationNumber: string;
  clientName: string;
  sender: string;
  pickupAddress: string;
  recipient: string;
  deliveryAddress: string;
  requestDate: string;
  scheduledPickupDate: string;
  priority: string;
  status: string;
  notes?: string;
}

export interface CreatePickupRequest {
  sender: string;
  pickupAddress: string;
  recipient: string;
  deliveryAddress: string;
  scheduledPickupDate: string;
  priority: string;
  notes?: string;
}

export type UserRole = 'Colaborador' | 'Cliente' | 'Motorista';
