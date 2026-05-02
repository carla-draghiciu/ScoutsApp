import { Injectable } from '@angular/core';

export interface ChatMessage {
  id: number;
  roomId: string;
  senderId: number;
  senderName: string;
  content: string;
  timestamp: string;
}

@Injectable({
  providedIn: 'root',
})
export class Chat {}
