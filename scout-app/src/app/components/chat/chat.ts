import { Component, OnInit, OnDestroy } from '@angular/core';
import { ChatMessage, ChatService } from '../../services/chat';
import { forkJoin, Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth';
import { environment } from '../../../environments/environment';
import { NgZone } from '@angular/core';

export interface PastChat {
  id: string | undefined;
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
  organizerId?: string;
  connectionStarted = false;

  env = environment.apiUrl;
  
  currentUser = {
    id: Number(localStorage.getItem('userId')),
    name: localStorage.getItem('userName') || 'Unknown User'
  };
  private subs = new Subscription();

  pastChats: PastChat[] = [];

  constructor(
    private http: HttpClient, 
    private cdr: ChangeDetectorRef, 
    private chatService: ChatService, 
    private route: ActivatedRoute, 
    private userService: AuthService,
    private ngZone: NgZone
  ) { }

  async ngOnInit() {
    this.subscribeToChat();
    await this.chatService.startConnection();
    this.connectionStarted = true;
    await this.loadPastConversations();
    this.handleRouteParams();
  }

  private subscribeToChat() {
    this.subs.add(
      this.chatService.historyLoaded$.subscribe(history =>{ this.ngZone.run(() => {
        this.messages = history;
        this.cdr.detectChanges();
      }); })
    );

    this.subs.add(
      this.chatService.messageReceived$.subscribe(message => {
      //   this.messages.push(message);
      //   const chatInSidebar = this.pastChats.find(c => c.id === this.organizerId);
      //   if (chatInSidebar) {
      //     chatInSidebar.lastMessage = message.content;
      //     chatInSidebar.time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      //   }
      // })
      this.ngZone.run(() => {
        this.messages = [...this.messages, message]; // create new array reference
        const chatInSidebar = this.pastChats.find(c => c.id === this.organizerId);
        if (chatInSidebar) {
          chatInSidebar.lastMessage = message.content;
          chatInSidebar.time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }
        this.cdr.detectChanges();
      });})
    );
  }

  private loadPastConversations(): Promise<void> {
    return new Promise(resolve => {
      this.http.get<ChatMessage[]>(`${this.env}/api/chat/conversations/${this.currentUser.id}`)
        .subscribe({
          next: conversations => {
            if (!conversations.length) { resolve(); return; }

            const otherUserIds = conversations.map(msg =>
              msg.roomId
                .replace('chat_', '')
                .replace(`_${this.currentUser.id}`, '')
                .replace(`${this.currentUser.id}_`, '')
            );

            forkJoin(
              otherUserIds.map(id => this.userService.getUserById(Number(id)))
            ).subscribe(users => {
              this.pastChats = conversations.map((msg, index) => ({
                id: otherUserIds[index],
                name: users[index]?.name || `User ${otherUserIds[index]}`,
                lastMessage: msg.content,
                time: new Date(msg.timeStamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
                unread: 0
              }));
              this.cdr.detectChanges();
              resolve();
            });
          },
          error: () => resolve() // don't block if it fails
        });
    });
  }

  private handleRouteParams() {
    this.route.queryParams.subscribe(async params => {
      await this.resolveActiveChat(params);
      await this.switchRoom();
    });
  }

  private async resolveActiveChat(params: any) {
    if (params['organizerId']) {
      this.organizerId = params['organizerId'];
      this.organizerName = params['organizerName'] || 'Organizer';
      this.cdr.detectChanges();

      if (!this.pastChats.find(c => c.id === this.organizerId)) {
        this.pastChats.unshift({
          id: this.organizerId,
          name: this.organizerName,
          lastMessage: 'Tap to chat...',
          time: 'Now',
          unread: 0
        });
      }
    } else if (this.pastChats.length > 0) {
      this.organizerId = this.pastChats[0].id;
      this.organizerName = this.pastChats[0].name;
    }
  }

  private async switchRoom() {
    if (!this.organizerId) return;

    const ids = [this.currentUser.id, this.organizerId].sort();
    const newRoomId = `chat_${ids[0]}_${ids[1]}`;

    if (this.roomId !== newRoomId) {
      if (this.roomId) this.chatService.leaveRoom(this.roomId);
      this.roomId = newRoomId;
      this.messages = [];
      await this.chatService.joinRoom(this.roomId);
    }
  }

  async selectChat(chat: PastChat) {
    if (this.organizerId === chat.id) return; // already in this chat

    this.organizerId = chat.id;
    this.organizerName = chat.name;

    if (this.roomId) {
      this.chatService.leaveRoom(this.roomId);
    }

    this.messages = []; // clear first
    this.cdr.detectChanges(); // force UI update

    const ids = [String(this.currentUser.id), String(this.organizerId)].sort();
    this.roomId = `chat_${ids[0]}_${ids[1]}`;

    await this.chatService.joinRoom(this.roomId);
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
