import { Routes } from '@angular/router';
import { ContactList } from './components/contact-list/contact-list';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/contacts',
    pathMatch: 'full'
  },
  {
    path: 'contacts',
    component: ContactList
  }
];
