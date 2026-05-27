import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { ChatMessage } from '../models/chat.model';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  env = environment.apiUrl;
  private hub!: signalR.HubConnection;

  messageReceived$ = new Subject<ChatMessage>();
  historyLoaded$ = new Subject<ChatMessage[]>();
  
  startConnection(): Promise<void> {
    console.log("start conn for chat");
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`${this.env}/hubs/chat`)
      .withAutomaticReconnect()
      .build();

    console.log("start conn for chat2");
    this.hub.on('ReceiveMessage', (message: ChatMessage) => {
      console.log('ReceiveMessage fired:', message);
      this.messageReceived$.next(message);
    });
    console.log("start conn for chat3");

    this.hub.on('LoadChatHistory', (messages: ChatMessage[]) => {
      console.log('LoadHistory fired:', messages);
      this.historyLoaded$.next(messages.reverse());
    });
    console.log("start conn for chat4");

    return this.hub.start();
  }

  joinRoom(roomId: string) {
    return this.hub.invoke('JoinRoom', roomId);
  }

  leaveRoom(roomId: string) {
    return this.hub.invoke('LeaveRoom', roomId);
  }

  sendMessage(roomId: string, senderId: number, senderName: string, content: string) {
    return this.hub.invoke('SendMessage', roomId, senderId, senderName, content);
  }

  stopConnection() {
    return this.hub?.stop();
  }
}
