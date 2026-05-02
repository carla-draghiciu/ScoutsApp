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
  
  startConnection(): Promise<void> {
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:500/hubs/chat')
      .withAutomaticReconnect()
      .build();

    this.hub.on('ReceiveMessage', (message: ChatMessage) => {
      this.messageReceived$.next(message);
    });

    this.hub.on('LoadChatHistory', (messages: ChatMessage[]) => {
      this.historyLoaded$.next(messages.reverse());
    });

    return this.hub.start();
  }

}
