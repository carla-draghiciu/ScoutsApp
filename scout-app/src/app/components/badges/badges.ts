import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PermissionService } from '../../services/permission';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-badges',
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './badges.html',
  styleUrl: './badges.css',
})
export class Badges {
  canManageBadges: boolean = false;
  isAdmin: boolean = false;
  
  constructor(private permissionService: PermissionService) { 
    this.canManageBadges = this.permissionService.hasPermission('manage_badges');
    this.isAdmin = this.permissionService.isAdmin();
  }
}
