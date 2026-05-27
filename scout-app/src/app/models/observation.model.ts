interface ObservationEntry {
  id: number;
  userId: number;
  user: { name: string; email: string };
  reason: string;
  flaggedAt: string;
  isResolved: boolean;
}