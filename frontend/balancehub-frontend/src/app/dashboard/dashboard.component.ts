import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MenubarModule } from 'primeng/menubar';
import { MenuItem } from 'primeng/api';
import { AuthService, User } from '../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    MenubarModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  currentUser: User | null = null;
  menuItems: MenuItem[] = [];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.setupMenu();
  }

  private setupMenu(): void {
    this.menuItems = [
      {
        label: 'Dashboard',
        icon: 'pi pi-home',
        routerLink: '/dashboard'
      },
      {
        label: 'Tasks',
        icon: 'pi pi-list',
        routerLink: '/tasks'
      },
      {
        label: 'Profile',
        icon: 'pi pi-user',
        items: [
          {
            label: 'View Profile',
            icon: 'pi pi-eye',
            command: () => this.viewProfile()
          },
          {
            label: 'Settings',
            icon: 'pi pi-cog',
            command: () => this.openSettings()
          }
        ]
      },
      {
        label: 'Logout',
        icon: 'pi pi-sign-out',
        command: () => this.logout()
      }
    ];
  }

  logout(): void {
    this.authService.logout();
  }

  viewProfile(): void {
    this.router.navigate(['/profile']);
  }

  openSettings(): void {
    this.router.navigate(['/settings']);
  }

  getWelcomeMessage(): string {
    if (this.currentUser) {
      const firstName = this.currentUser.firstName || this.currentUser.email.split('@')[0];
      return `Welcome back, ${firstName}!`;
    }
    return 'Welcome to BalanceHub!';
  }

  getUserRole(): string {
    return this.currentUser?.role || 'User';
  }
}
