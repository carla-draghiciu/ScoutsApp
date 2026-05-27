export interface RegisteringUser {
  name: string;
  email: string;
  scoutId: string;
  password: string;
  dateOfBirth: string;
  scoutLevel: string;
}

export interface LoggingUser {
  identifier: string;
  password: string;
}

export interface UserProfile {
  id: number;
  name: string;
  email: string;
  dateOfBirth: string;
  scoutLevel: string;
}