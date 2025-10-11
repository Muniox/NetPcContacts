import {Component, inject, OnInit} from '@angular/core';

import {AuthService} from '../../services/auth-service';
import {LoginRequest} from '../../models/login-request';

@Component({
  selector: 'app-login',
  imports: [],
  template:
    `
      <p>login works!</p>
    `,
  styles: [],
  host: {}
})
export class Login implements OnInit{
  authService = inject(AuthService);
  credentials: LoginRequest = {
    email: '',
    password: ''
  }

  ngOnInit(): void {
    this.isLoggedIn();
  }

  isLoggedIn() {
    return this.authService.isLoggedIn();
  }

  login() {
    this.authService.login(this.credentials)
      .subscribe(() => {
        console.log('logged in');
      })
  }
}
