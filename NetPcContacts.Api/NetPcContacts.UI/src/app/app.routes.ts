import { Routes } from '@angular/router';
import { ContactList } from './components/contact-list/contact-list';
import { Login } from './components/login/login';

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
    component: Login
  },
  {
    path: 'register',
    loadComponent: () => import('./components/register/register').then(m => m.Register)
  }
];
