import { Component, inject, signal, HostListener } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { AuthService } from './services/auth-service';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatSidenavModule
  ],
  template:
    `
      <div class="app-container">
        <mat-toolbar class="navbar">
          <div class="navbar-content">
            <div class="navbar-brand">
              <mat-icon class="brand-icon">contacts</mat-icon>
              <span class="brand-title">NetPcContacts</span>
            </div>

            <!-- Hamburger menu button for mobile -->
            @if (isMobile()) {
              <button mat-icon-button (click)="toggleSidenav()" class="menu-button">
                <mat-icon>menu</mat-icon>
              </button>
            }

            <!-- Desktop navigation -->
            @if (!isMobile()) {
              <nav class="navbar-menu">
                <a mat-button routerLink="/contacts" routerLinkActive="active-link" [routerLinkActiveOptions]="{exact: false}">
                  <mat-icon>contacts</mat-icon>
                  Kontakty
                </a>

                @if (authService.isLoggedIn()) {
                  <button mat-button [matMenuTriggerFor]="userMenu">
                    <mat-icon>account_circle</mat-icon>
                    <span>Konto</span>
                    <mat-icon>arrow_drop_down</mat-icon>
                  </button>

                  <mat-menu #userMenu="matMenu">
                    <button mat-menu-item (click)="logout()">
                      <mat-icon>logout</mat-icon>
                      <span>Wyloguj</span>
                    </button>
                  </mat-menu>
                } @else {
                  <a mat-button routerLink="/login">
                    <mat-icon>login</mat-icon>
                    Zaloguj
                  </a>
                  <a mat-button routerLink="/register" class="register-nav-button">
                    <mat-icon>person_add</mat-icon>
                    Zarejestruj się
                  </a>
                }
              </nav>
            }
          </div>
        </mat-toolbar>

        <mat-sidenav-container class="sidenav-container">
          <mat-sidenav #sidenav mode="over" position="end" [opened]="sidenavOpened()">
            <div class="sidenav-content">
              <div class="sidenav-header">
                <span class="sidenav-title">Menu</span>
                <button mat-icon-button (click)="toggleSidenav()">
                  <mat-icon>close</mat-icon>
                </button>
              </div>

              <nav class="sidenav-menu">
                <a mat-button routerLink="/contacts" routerLinkActive="active-link" (click)="closeSidenav()" [routerLinkActiveOptions]="{exact: false}">
                  <mat-icon>contacts</mat-icon>
                  <span>Kontakty</span>
                </a>

                @if (authService.isLoggedIn()) {
                  <button mat-button (click)="logout(); closeSidenav()">
                    <mat-icon>logout</mat-icon>
                    <span>Wyloguj</span>
                  </button>
                } @else {
                  <a mat-button routerLink="/login" (click)="closeSidenav()">
                    <mat-icon>login</mat-icon>
                    <span>Zaloguj</span>
                  </a>
                  <a mat-button routerLink="/register" (click)="closeSidenav()">
                    <mat-icon>person_add</mat-icon>
                    <span>Zarejestruj się</span>
                  </a>
                }
              </nav>
            </div>
          </mat-sidenav>

          <mat-sidenav-content>
            <main class="main-content">
              <router-outlet />
            </main>

            <footer class="footer">
              <div class="footer-content">
                <div class="footer-section">
                  <mat-icon class="footer-icon">contacts</mat-icon>
                  <span class="footer-title">NetPcContacts</span>
                </div>
                <div class="footer-section">
                  <span class="footer-text">&copy; {{ currentYear }} NetPcContacts. Wszystkie prawa zastrzeżone.</span>
                </div>
                <div class="footer-section footer-links">
                  <a href="https://github.com" target="_blank" rel="noopener noreferrer" class="footer-link">
                    <mat-icon>code</mat-icon>
                    GitHub
                  </a>
                </div>
              </div>
            </footer>
          </mat-sidenav-content>
        </mat-sidenav-container>
      </div>
    `,
  styles: [`
    .app-container {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      background-color: theme('colors.azure-neutral.99');
    }

    :host {
      display: block;
      min-height: 100vh;
    }

    .navbar {
      background: linear-gradient(135deg, theme('colors.blue-primary.50') 0%, theme('colors.blue-primary.60') 100%) !important;
      color: theme('colors.blue-neutral.100') !important;
      box-shadow: 0 2px 8px rgba(90, 100, 255, 0.2);
      position: sticky;
      top: 0;
      z-index: 1000;
    }

    .navbar-content {
      width: 100%;
      max-width: 1400px;
      margin: 0 auto;
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0 16px;
    }

    .navbar-brand {
      display: flex;
      align-items: center;
      gap: 12px;
      font-size: 20px;
      font-weight: 600;
      color: theme('colors.blue-neutral.100');
    }

    .brand-icon {
      font-size: 28px;
      width: 28px;
      height: 28px;
      color: theme('colors.blue-neutral.100');
    }

    .brand-title {
      font-weight: 700;
      letter-spacing: 0.5px;
    }

    .navbar-menu {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .navbar-menu .mdc-button {
      color: theme('colors.blue-neutral.100') !important;
    }

    ::ng-deep .active-link:hover {
      background-color: rgba(255, 255, 255, 0.1) !important;
    }

    .main-content {
      flex: 1;
      width: 100%;
    }

    .menu-button {
      color: theme('colors.blue-neutral.100') !important;
    }

    .sidenav-container {
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    ::ng-deep .mat-drawer-container {
      background-color: transparent !important;
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    ::ng-deep .mat-drawer-content {
      display: flex !important;
      flex-direction: column !important;
      flex: 1 !important;
    }

    ::ng-deep mat-sidenav-content {
      display: flex !important;
      flex-direction: column !important;
      flex: 1 !important;
    }

    ::ng-deep .mat-drawer {
      width: 280px;
      background-color: theme('colors.blue-neutral.100');
    }

    .sidenav-content {
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    .sidenav-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px;
      background: linear-gradient(135deg, theme('colors.blue-primary.50') 0%, theme('colors.blue-primary.60') 100%);
      color: theme('colors.blue-neutral.100');
    }

    .sidenav-title {
      font-size: 20px;
      font-weight: 600;
    }

    .sidenav-header button {
      color: theme('colors.blue-neutral.100') !important;
    }

    .sidenav-menu {
      display: flex;
      flex-direction: column;
      padding: 8px;
      gap: 4px;
    }

    .sidenav-menu a,
    .sidenav-menu button {
      justify-content: flex-start;
      padding: 12px 16px;
      width: 100%;
      text-align: left;
      color: theme('colors.blue-neutral.20') !important;
    }

    .sidenav-menu .active-link {
      background-color: theme('colors.blue-primary.95');
      color: theme('colors.blue-primary.30') !important;
    }

    .sidenav-menu mat-icon {
      margin-right: 12px;
      color: theme('colors.blue-primary.50');
    }

    .sidenav-menu .active-link mat-icon {
      color: theme('colors.blue-primary.30');
    }

    ::ng-deep .user-menu {
      margin-top: 8px;
    }

    ::ng-deep .mat-mdc-menu-content {
      padding: 8px 0;
    }

    ::ng-deep .mat-mdc-menu-item {
      color: theme('colors.blue-neutral.20');
    }

    ::ng-deep .mat-mdc-menu-item:hover {
      background-color: theme('colors.blue-primary.95');
    }

    ::ng-deep .mat-mdc-menu-item mat-icon {
      color: theme('colors.blue-primary.50');
      margin-right: 12px;
    }

    .footer {
      background: linear-gradient(135deg, theme('colors.blue-primary.50') 0%, theme('colors.blue-primary.60') 100%);
      color: theme('colors.blue-neutral.100');
      padding: 24px 16px;
      margin-top: auto;
      box-shadow: 0 -2px 8px rgba(90, 100, 255, 0.2);
    }

    .footer-content {
      width: 100%;
      max-width: 1400px;
      margin: 0 auto;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 24px;
      flex-wrap: wrap;
    }

    .footer-section {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .footer-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
      color: theme('colors.blue-neutral.100');
    }

    .footer-title {
      font-weight: 600;
      font-size: 16px;
      color: theme('colors.blue-neutral.100');
    }

    .footer-text {
      font-size: 14px;
      color: theme('colors.blue-primary.95');
    }

    .footer-links {
      gap: 16px;
    }

    .footer-link {
      display: flex;
      align-items: center;
      gap: 6px;
      color: theme('colors.blue-neutral.100');
      text-decoration: none;
      font-size: 14px;
      transition: opacity 0.2s ease;
    }

    .footer-link:hover {
      opacity: 0.8;
    }

    .footer-link mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    @media (max-width: 768px) {
      .footer-content {
        flex-direction: column;
        text-align: center;
        gap: 16px;
      }

      .footer-section {
        justify-content: center;
      }
    }
  `],
  host: {}
})
export class App {
  protected readonly title = signal('NetPcContacts.UI');
  protected readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly currentYear = new Date().getFullYear();

  // Mobile detection
  protected readonly isMobile = signal(false);
  protected readonly sidenavOpened = signal(false);

  constructor() {
    this.checkScreenSize();
  }

  @HostListener('window:resize')
  onResize() {
    this.checkScreenSize();
  }

  private checkScreenSize(): void {
    this.isMobile.set(window.innerWidth < 768);
    // Close sidenav when switching to desktop
    if (!this.isMobile() && this.sidenavOpened()) {
      this.sidenavOpened.set(false);
    }
  }

  toggleSidenav(): void {
    this.sidenavOpened.update(opened => !opened);
  }

  closeSidenav(): void {
    this.sidenavOpened.set(false);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
