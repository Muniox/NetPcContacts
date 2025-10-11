import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template:
    `
      <h1 class="h-1">Test</h1>

      <router-outlet />
    `,
  styles: [],
  host: {}
})
export class App {
  protected readonly title = signal('NetPcContacts.UI');
}
