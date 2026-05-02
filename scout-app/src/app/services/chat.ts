import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

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
export class ChatService {
  private hub!: signalR.HubConnection;

  messageReceived$ = new Subject<ChatMessage>();
  historyLoaded$ = new Subject<ChatMessage[]>();
  
  constructor() {
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:500/hubs/chat')
      .withAutomaticReconnect()
      .build();
  }
}
