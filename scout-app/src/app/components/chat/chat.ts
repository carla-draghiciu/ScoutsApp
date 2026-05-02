import { Component, OnInit, OnDestroy } from '@angular/core';
import { ChatMessage, ChatService } from '../../services/chat';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';

export interface PastChat {
  id: number;
  name: string;
  lastMessage: string;
  time: string;
  unread: number;
}

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class Chat implements OnInit, OnDestroy {
  messages: ChatMessage[] = [];
  newMessage = '';
  roomId = '';
  organizerName = 'Organizer';
  organizerId?: number;
  connectionStarted = false;
  
  currentUser = {
    id: Number(localStorage.getItem('userId')),
    name: localStorage.getItem('userName') || 'Unknown User'
  };
  private subs = new Subscription();

  // Mocked list of past conversations
  pastChats: PastChat[] = [
    { id: 101, name: 'Alice Smith', lastMessage: 'Are we still meeting today?', time: '10:30 AM', unread: 2 },
    { id: 102, name: 'Bob Jones', lastMessage: 'Thanks for organizing!', time: 'Yesterday', unread: 0 },
    { id: 103, name: 'Charlie Brown', lastMessage: 'See you at the event.', time: 'Monday', unread: 0 },
  ];

  constructor(private chatService: ChatService, private route: ActivatedRoute) { }

  async ngOnInit() {
    this.subs.add(
      this.chatService.historyLoaded$.subscribe(history => this.messages = history)
    );

    this.subs.add(
      this.chatService.messageReceived$.subscribe(message => {
        this.messages.push(message);
        // update last message in sidebar
        const chatInSidebar = this.pastChats.find(c => c.id === this.organizerId);
        if (chatInSidebar) {
          chatInSidebar.lastMessage = message.content;
          chatInSidebar.time = new Date().toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
        }
      })
    );

    await this.chatService.startConnection();
    this.connectionStarted = true;

    this.route.queryParams.subscribe(async params => {
      if (params['organizerId']) {
        this.organizerId = Number(params['organizerId']);
        this.organizerName = params['organizerName'] || 'Organizer';
        
        // Add current chat to the top of past chats if it's not there
        if (!this.pastChats.find(c => c.id === this.organizerId)) {
          this.pastChats.unshift({
            id: this.organizerId,
            name: this.organizerName,
            lastMessage: 'Tap to chat...',
            time: 'Now',
            unread: 0
          });
        }
      } else {
        this.organizerId = this.pastChats[0].id;
        this.organizerName = this.pastChats[0].name;
      }
      
      const minId = Math.min(this.currentUser.id, this.organizerId || 0);
      const maxId = Math.max(this.currentUser.id, this.organizerId || 0);
      const newRoomId = `chat_${minId}_${maxId}`;

      if (this.roomId !== newRoomId) {
        if (this.roomId) {
          this.chatService.leaveRoom(this.roomId);
        }
        this.roomId = newRoomId;
        this.messages = []; // Clear current messages
        if (this.connectionStarted) {
          await this.chatService.joinRoom(this.roomId);
        }
      }
    });
  }

  async selectChat(chat: PastChat) {
    this.organizerId = chat.id;
    this.organizerName = chat.name;
    const minId = Math.min(this.currentUser.id, this.organizerId);
    const maxId = Math.max(this.currentUser.id, this.organizerId);
    
    const newRoomId = `chat_${minId}_${maxId}`;
    if (this.roomId !== newRoomId) {
      if (this.roomId) {
        this.chatService.leaveRoom(this.roomId);
      }
      this.roomId = newRoomId;
      this.messages = []; 
      await this.chatService.joinRoom(this.roomId);
    }
  }

  async send() {
    const text = this.newMessage.trim();
    if (!text) return;
    await this.chatService.sendMessage(this.roomId, this.currentUser.id, this.currentUser.name, text);
    this.newMessage = '';
  }

  ngOnDestroy() {
    if (this.roomId) {
      this.chatService.leaveRoom(this.roomId);
    }
    this.chatService.stopConnection();
    this.subs.unsubscribe();
  }
}
