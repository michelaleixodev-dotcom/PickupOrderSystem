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

export interface StatusHistoryItem {
  fromStatus?: string;
  toStatus: string;
  changedAt: string;
  changedBy: string;
}

export interface OccurrenceItem {
  id: string;
  type: string;
  description: string;
  occurrenceDate: string;
  registeredBy: string;
  resolved: boolean;
  resolutionNotes?: string;
}

export interface AssignmentInfo {
  id: string;
  driverName: string;
  vehiclePlate: string;
  vehicleModel: string;
  assignmentDate: string;
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
  assignment?: AssignmentInfo;
  statusHistory?: StatusHistoryItem[];
  occurrences?: OccurrenceItem[];
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

export interface DriverOption {
  id: string;
  name: string;
}

export interface VehicleOption {
  id: string;
  model: string;
  licensePlate: string;
}

export type UserRole = 'Colaborador' | 'Cliente' | 'Motorista';
