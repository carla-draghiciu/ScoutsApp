export interface ChatMessage {
  id: string;
  roomId: string;
  senderId: number;
  senderName: string;
  content: string;
  timeStamp: string;
}