import {Component, inject, OnInit, ViewChild, computed} from '@angular/core';
import {FormControl, ReactiveFormsModule} from '@angular/forms';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatIconModule} from '@angular/material/icon';
import {MatTableModule} from '@angular/material/table';
import {MatPaginator, MatPaginatorModule, PageEvent} from '@angular/material/paginator';
import {MatSortModule, Sort, SortDirection as MatSortDirection} from '@angular/material/sort';
import {MatCardModule} from '@angular/material/card';
import {MatChipsModule} from '@angular/material/chips';
import {MatSnackBar, MatSnackBarModule} from '@angular/material/snack-bar';
import {MatDialog, MatDialogModule} from '@angular/material/dialog';
import {debounceTime, distinctUntilChanged} from 'rxjs';

import {ContactService} from '../../services/contact.service';
import {AuthService} from '../../services/auth-service';
import {SortDirection} from '../../models';
import {ContactDialog} from '../contact-dialog/contact-dialog';

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
    MatSortModule,
    MatCardModule,
    MatChipsModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  template: `
    <div class="container mx-auto px-4 py-8 max-w-7xl">
      <div class="header-section">
        <h1 class="mat-headline-4">Lista Kontaktów</h1>
        @if (authService.isLoggedIn()) {
          <button mat-fab color="primary" class="add-button" (click)="openAddContactDialog()" aria-label="Dodaj kontakt">
            <mat-icon>add</mat-icon>
          </button>
        }
      </div>

      <!-- Search bar -->
      <mat-card class="mb-6">
        <mat-card-content class="pt-4">
          <mat-form-field class="w-full" appearance="outline">
            <mat-label>Szukaj</mat-label>
            <input
              matInput
              [formControl]="searchControl"
              placeholder="Imię, nazwisko lub email"
            />
            <mat-icon matPrefix>search</mat-icon>
          </mat-form-field>
        </mat-card-content>
      </mat-card>

      <!-- Loading spinner -->
      @if (contactService.loading()) {
        <div class="flex justify-center items-center py-12">
          <mat-spinner color="primary"></mat-spinner>
        </div>
      }

      <!-- Table -->
      @if (!contactService.loading() && contactService.contacts()) {
        <mat-card>
          <table
            mat-table
            [dataSource]="contactService.contacts()!.items"
            matSort
            [matSortActive]="currentSortColumn()"
            [matSortDirection]="currentSortDirection()"
            matSortDisableClear
            (matSortChange)="onSortChange($event)"
            class="w-full"
          >
            <!-- ID Column -->
            <ng-container matColumnDef="id">
              <th mat-header-cell *matHeaderCellDef>ID</th>
              <td mat-cell *matCellDef="let contact">{{ contact.id }}</td>
            </ng-container>

            <!-- Name Column -->
            <ng-container matColumnDef="name">
              <th mat-header-cell *matHeaderCellDef mat-sort-header>Imię</th>
              <td mat-cell *matCellDef="let contact">{{ contact.name }}</td>
            </ng-container>

            <!-- Surname Column -->
            <ng-container matColumnDef="surname">
              <th mat-header-cell *matHeaderCellDef mat-sort-header>Nazwisko</th>
              <td mat-cell *matCellDef="let contact">{{ contact.surname }}</td>
            </ng-container>

            <!-- Email Column -->
            <ng-container matColumnDef="email">
              <th mat-header-cell *matHeaderCellDef>Email</th>
              <td mat-cell *matCellDef="let contact">
                <div class="flex items-center gap-2">
                  <mat-icon class="icon-sm">email</mat-icon>
                  {{ contact.email }}
                </div>
              </td>
            </ng-container>

            <!-- Phone Column -->
            <ng-container matColumnDef="phoneNumber">
              <th mat-header-cell *matHeaderCellDef>Telefon</th>
              <td mat-cell *matCellDef="let contact">
                <div class="flex items-center gap-2">
                  <mat-icon class="icon-sm">phone</mat-icon>
                  {{ contact.phoneNumber }}
                </div>
              </td>
            </ng-container>

            <!-- Category Column -->
            <ng-container matColumnDef="category">
              <th mat-header-cell *matHeaderCellDef mat-sort-header>Kategoria</th>
              <td mat-cell *matCellDef="let contact">
                <mat-chip>{{ contact.category }}</mat-chip>
              </td>
            </ng-container>

            <!-- Actions Column -->
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Akcje</th>
              <td mat-cell *matCellDef="let contact">
                <button mat-icon-button color="primary" [attr.aria-label]="'Zobacz szczegóły kontaktu ' + contact.name">
                  <mat-icon>visibility</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns()"></tr>
            <tr
              mat-row
              *matRowDef="let row; columns: displayedColumns();"
              class="cursor-pointer"
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
          >
          </mat-paginator>
        </mat-card>
      }

      <!-- Empty state -->
      @if (!contactService.loading() && contactService.contacts() && contactService.contacts()!.items.length === 0) {
        <mat-card>
          <mat-card-content class="text-center py-12">
            <mat-icon class="empty-state-icon mb-4" color="disabled">person_off</mat-icon>
            <h2 class="mat-headline-6 mb-2">Brak kontaktów</h2>
            <p class="mat-body-1">Nie znaleziono żadnych kontaktów spełniających kryteria wyszukiwania.</p>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .header-section {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
    }

    .add-button {
      position: fixed;
      bottom: 24px;
      right: 24px;
      z-index: 1000;
    }

    .icon-sm {
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    .empty-state-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
    }

    .cursor-pointer {
      cursor: pointer;
    }

    .cursor-pointer:hover {
      background-color: rgba(0, 0, 0, 0.04);
    }

    @media (max-width: 768px) {
      .header-section {
        flex-direction: column;
        align-items: flex-start;
        gap: 16px;
      }

      .add-button {
        bottom: 16px;
        right: 16px;
      }
    }
  `],
  host: {}
})
export class ContactList implements OnInit {
  contactService = inject(ContactService);
  authService = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  // Table columns - computed based on auth state
  displayedColumns = computed(() => {
    const baseColumns = ['id', 'name', 'surname', 'email', 'phoneNumber', 'category'];
    return this.authService.isLoggedIn()
      ? [...baseColumns, 'actions']
      : baseColumns;
  });

  // Form controls
  searchControl = new FormControl('');

  // Expose SortDirection enum to template
  SortDirection = SortDirection;

  // Computed properties for MatSort bindings
  currentSortColumn = computed(() => {
    const sortBy = this.contactService.sortBy();
    const columnMap: { [key: string]: string } = {
      'Name': 'name',
      'Surname': 'surname',
      'Category': 'category'
    };
    return sortBy ? columnMap[sortBy] : '';
  });

  currentSortDirection = computed((): MatSortDirection => {
    const direction = this.contactService.sortDirection();
    return direction === SortDirection.Ascending ? 'asc' : 'desc';
  });

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
      this.contactService.setSortDirection(SortDirection.Ascending);
      this.contactService.loadContacts();
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
      const direction = sort.direction === 'asc' ? SortDirection.Ascending : SortDirection.Descending;
      this.contactService.setSortBy(apiSortField);
      this.contactService.setSortDirection(direction);
      this.contactService.loadContacts();
    }
  }

  openAddContactDialog(): void {
    const dialogRef = this.dialog.open(ContactDialog, {
      width: '600px',
      maxWidth: '90vw',
      disableClose: false,
      autoFocus: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        this.snackBar.open('Kontakt został dodany pomyślnie!', 'Zamknij', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top'
        });
      }
    });
  }
}