import { Component } from '@angular/core';
import { ChatMessage, ChatService } from '../../services/chat';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-chat',
  imports: [],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class Chat implements OnInit, OnDestroy {
  messages: ChatMessage[] = [];
  newMessage = '';
  roomId = 'general';
  currentUser = {
    id: Number(localStorage.getItem('userId')),
    name: localStorage.getItem('userName') || 'Unknown User'
  };
  private subs = new Subscription();

  constructor(private chatService: ChatService) { }

  async ngOnInit() {
    await this.chatService.startConnection();
    await this.chatService.joinRoom(this.roomId);

    this.subs.add(
      this.chatService.historyLoaded$.subscribe(history => this.messages = history)
    );

    this.subs.add(
      this.chatService.messageReceived$.subscribe(message => this.messages.push(message))
    );
  }

  async send() {
    const text = this.newMessage.trim();
    if (!text) return;
    await this.chatService.sendMessage(this.roomId, this.currentUser.id, this.currentUser.name, text);
    this.newMessage = '';
  }

  ngOnDestroy() {
    this.chatService.leaveRoom(this.roomId);
    this.chatService.stopConnection();
    this.subs.unsubscribe();
  }
}
