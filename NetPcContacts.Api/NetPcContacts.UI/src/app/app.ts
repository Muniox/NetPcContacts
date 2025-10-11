import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template:
    `
      <router-outlet />
    `,
  styles: [],
  host: {}
})
export class App {
  protected readonly title = signal('NetPcContacts.UI');
}
