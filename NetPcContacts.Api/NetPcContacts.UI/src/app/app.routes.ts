import { Routes } from '@angular/router';
import { ContactList } from './components/contact-list/contact-list';
import { Login } from './components/login/login';
import { guestGuard } from './guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/contacts',
    pathMatch: 'full'
  },
  {
    path: 'contacts',
    component: ContactList
  },
  {
    path: 'login',
    component: Login,
    canActivate: [guestGuard]
  },
  {
    path: 'register',
    loadComponent: () => import('./components/register/register').then(m => m.Register),
    canActivate: [guestGuard]
  }
];
