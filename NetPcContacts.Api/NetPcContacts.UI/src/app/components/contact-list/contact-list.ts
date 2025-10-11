import {Component, inject, OnInit, ViewChild} from '@angular/core';
import {FormControl, ReactiveFormsModule} from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatIconModule} from '@angular/material/icon';
import {MatTableModule} from '@angular/material/table';
import {MatPaginator, MatPaginatorModule, PageEvent} from '@angular/material/paginator';
import {MatSortModule, Sort} from '@angular/material/sort';
import {debounceTime, distinctUntilChanged} from 'rxjs';

import {ContactService} from '../../services/contact.service';
import {SortDirection} from '../../models';

@Component({
  selector: 'app-contact-list',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule
  ],
  template: `
    <div class="container mx-auto px-4 py-8">
      <h1 class="text-3xl font-bold mb-8 text-gray-800">Lista Kontaktów</h1>

      <!-- Search bar -->
      <div class="bg-white rounded-lg shadow-md p-6 mb-6">
        <mat-form-field class="w-full" appearance="outline">
          <mat-label>Szukaj</mat-label>
          <input
            matInput
            [formControl]="searchControl"
            placeholder="Imię, nazwisko lub email"
          />
          <mat-icon matPrefix>search</mat-icon>
        </mat-form-field>
      </div>

      <!-- Loading spinner -->
      @if (contactService.loading()) {
        <div class="flex justify-center items-center py-12">
          <mat-spinner></mat-spinner>
        </div>
      }

      <!-- Error message -->
      @if (contactService.error()) {
        <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-6">
          <strong>Błąd:</strong> {{ contactService.error() }}
        </div>
      }

      <!-- Table -->
      @if (!contactService.loading() && contactService.contacts()) {
        <div class="bg-white rounded-lg shadow-md overflow-hidden">
          <table
            mat-table
            [dataSource]="contactService.contacts()!.items"
            matSort
            (matSortChange)="onSortChange($event)"
            class="w-full"
          >
            <!-- ID Column -->
            <ng-container matColumnDef="id">
              <th mat-header-cell *matHeaderCellDef class="bg-gray-50">ID</th>
              <td mat-cell *matCellDef="let contact">{{ contact.id }}</td>
            </ng-container>

            <!-- Name Column -->
            <ng-container matColumnDef="name">
              <th mat-header-cell *matHeaderCellDef mat-sort-header class="bg-gray-50">Imię</th>
              <td mat-cell *matCellDef="let contact">{{ contact.name }}</td>
            </ng-container>

            <!-- Surname Column -->
            <ng-container matColumnDef="surname">
              <th mat-header-cell *matHeaderCellDef mat-sort-header class="bg-gray-50">Nazwisko</th>
              <td mat-cell *matCellDef="let contact">{{ contact.surname }}</td>
            </ng-container>

            <!-- Email Column -->
            <ng-container matColumnDef="email">
              <th mat-header-cell *matHeaderCellDef class="bg-gray-50">Email</th>
              <td mat-cell *matCellDef="let contact">
                <div class="flex items-center">
                  <mat-icon class="mr-2 text-gray-500 text-sm">email</mat-icon>
                  {{ contact.email }}
                </div>
              </td>
            </ng-container>

            <!-- Phone Column -->
            <ng-container matColumnDef="phoneNumber">
              <th mat-header-cell *matHeaderCellDef class="bg-gray-50">Telefon</th>
              <td mat-cell *matCellDef="let contact">
                <div class="flex items-center">
                  <mat-icon class="mr-2 text-gray-500 text-sm">phone</mat-icon>
                  {{ contact.phoneNumber }}
                </div>
              </td>
            </ng-container>

            <!-- Category Column -->
            <ng-container matColumnDef="category">
              <th mat-header-cell *matHeaderCellDef mat-sort-header class="bg-gray-50">Kategoria</th>
              <td mat-cell *matCellDef="let contact">
                <span class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800">
                  {{ contact.category }}
                </span>
              </td>
            </ng-container>

            <!-- Actions Column -->
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef class="bg-gray-50">Akcje</th>
              <td mat-cell *matCellDef="let contact">
                <button mat-icon-button color="primary">
                  <mat-icon>visibility</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr
              mat-row
              *matRowDef="let row; columns: displayedColumns;"
              class="hover:bg-gray-50 cursor-pointer"
            ></tr>
          </table>

          <!-- Paginator -->
          <mat-paginator
            [length]="contactService.contacts()?.totalItemsCount"
            [pageSize]="contactService.pageSize()"
            [pageIndex]="contactService.pageNumber() - 1"
            [pageSizeOptions]="[5, 10, 15, 30]"
            (page)="onPageChange($event)"
            showFirstLastButtons
            class="border-t"
          >
          </mat-paginator>
        </div>
      }

      <!-- Empty state -->
      @if (!contactService.loading() && contactService.contacts() && contactService.contacts()!.items.length === 0) {
        <div class="text-center py-12 bg-white rounded-lg shadow-md">
          <mat-icon class="text-6xl text-gray-400 mb-4">person_off</mat-icon>
          <h2 class="text-2xl font-semibold text-gray-600 mb-2">Brak kontaktów</h2>
          <p class="text-gray-500">Nie znaleziono żadnych kontaktów spełniających kryteria wyszukiwania.</p>
        </div>
      }
    </div>
  `,
  styles: [`
    ::ng-deep .mat-mdc-table {
      background: transparent;
    }

    ::ng-deep .mat-mdc-header-cell {
      font-weight: 600;
      color: #374151;
    }

    ::ng-deep .mat-mdc-cell {
      color: #4b5563;
    }

    ::ng-deep .mat-mdc-row:hover {
      background-color: #f9fafb;
    }
  `],
  host: {}
})
export class ContactList implements OnInit {
  contactService = inject(ContactService);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  // Table columns
  displayedColumns: string[] = ['id', 'name', 'surname', 'email', 'phoneNumber', 'category', 'actions'];

  // Form controls
  searchControl = new FormControl('');

  // Expose SortDirection enum to template
  SortDirection = SortDirection;

  constructor() {
    // Setup search with debounce
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged()
      )
      .subscribe(value => {
        this.contactService.setSearchPhrase(value || '');
        this.contactService.loadContacts();
      });
  }

  ngOnInit(): void {
    // Load initial data
    this.contactService.loadContacts();
  }

  onPageChange(event: PageEvent): void {
    this.contactService.setPageSize(event.pageSize);
    this.contactService.setPageNumber(event.pageIndex + 1);
    this.contactService.loadContacts();
  }

  onSortChange(sort: Sort): void {
    if (!sort.active || sort.direction === '') {
      this.contactService.setSortBy(undefined);
      return;
    }

    // Map table column names to API field names
    const sortFieldMap: { [key: string]: 'Name' | 'Surname' | 'Category' } = {
      'name': 'Name',
      'surname': 'Surname',
      'category': 'Category'
    };

    const apiSortField = sortFieldMap[sort.active];
    if (apiSortField) {
      this.contactService.setSortBy(apiSortField);
      this.contactService.setSortDirection(
        sort.direction === 'asc' ? SortDirection.Ascending : SortDirection.Descending
      );
      this.contactService.loadContacts();
    }
  }
}