import {Component, Inject, OnInit, signal} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogModule, MatDialogRef} from '@angular/material/dialog';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatDividerModule} from '@angular/material/divider';
import {MatChipsModule} from '@angular/material/chips';
import {DatePipe} from '@angular/common';

import {ContactService} from '../../services/contact.service';
import {Contact} from '../../models';

@Component({
  selector: 'app-contact-details-dialog',
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatChipsModule,
    DatePipe
  ],
  template: `
    <h2 mat-dialog-title class="dialog-title">
      <mat-icon>person</mat-icon>
      Szczegóły Kontaktu
    </h2>

    <mat-dialog-content class="dialog-content">
      @if (loading()) {
        <div class="loading-container">
          <mat-spinner diameter="50"></mat-spinner>
          <p>Ładowanie...</p>
        </div>
      } @else if (error()) {
        <div class="error-container">
          <mat-icon color="warn">error</mat-icon>
          <p>{{ error() }}</p>
        </div>
      } @else if (contact()) {
        <div class="details-grid">
          <!-- Personal Information -->
          <div class="section">
            <h3 class="section-title">Dane osobowe</h3>

            <div class="detail-row">
              <mat-icon class="detail-icon">badge</mat-icon>
              <div class="detail-content">
                <span class="detail-label">Imię i Nazwisko</span>
                <span class="detail-value">{{ contact()!.name }} {{ contact()!.surname }}</span>
              </div>
            </div>

            <div class="detail-row">
              <mat-icon class="detail-icon">cake</mat-icon>
              <div class="detail-content">
                <span class="detail-label">Data urodzenia</span>
                <span class="detail-value">{{ contact()!.birthDate | date:'dd.MM.yyyy' }}</span>
              </div>
            </div>
          </div>

          <mat-divider></mat-divider>

          <!-- Contact Information -->
          <div class="section">
            <h3 class="section-title">Dane kontaktowe</h3>

            <div class="detail-row">
              <mat-icon class="detail-icon">email</mat-icon>
              <div class="detail-content">
                <span class="detail-label">Email</span>
                <span class="detail-value">{{ contact()!.email }}</span>
              </div>
            </div>

            <div class="detail-row">
              <mat-icon class="detail-icon">phone</mat-icon>
              <div class="detail-content">
                <span class="detail-label">Telefon</span>
                <span class="detail-value">{{ contact()!.phoneNumber }}</span>
              </div>
            </div>
          </div>

          <mat-divider></mat-divider>

          <!-- Category Information -->
          <div class="section">
            <h3 class="section-title">Kategoria</h3>

            <div class="detail-row">
              <mat-icon class="detail-icon">category</mat-icon>
              <div class="detail-content">
                <span class="detail-label">Kategoria</span>
                <mat-chip class="category-chip">{{ contact()!.categoryName }}</mat-chip>
              </div>
            </div>

            @if (contact()!.subcategoryName) {
              <div class="detail-row">
                <mat-icon class="detail-icon">label</mat-icon>
                <div class="detail-content">
                  <span class="detail-label">Podkategoria</span>
                  <mat-chip class="subcategory-chip">{{ contact()!.subcategoryName }}</mat-chip>
                </div>
              </div>
            }

            @if (contact()!.customSubcategory) {
              <div class="detail-row">
                <mat-icon class="detail-icon">edit</mat-icon>
                <div class="detail-content">
                  <span class="detail-label">Własna podkategoria</span>
                  <span class="detail-value">{{ contact()!.customSubcategory }}</span>
                </div>
              </div>
            }
          </div>
        </div>
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onClose()">Zamknij</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-title {
      display: flex;
      align-items: center;
      gap: 12px;
      color: theme('colors.blue-primary.40');
      margin: 0;
      padding: 20px 24px;
    }

    .dialog-title mat-icon {
      font-size: 28px;
      width: 28px;
      height: 28px;
    }

    .dialog-content {
      min-width: 500px;
      max-width: 600px;
      padding: 0 24px 20px;
    }

    .loading-container,
    .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 40px 20px;
      gap: 16px;
    }

    .error-container mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
    }

    .details-grid {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .section {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .section-title {
      font-size: 1.1rem;
      font-weight: 600;
      color: theme('colors.blue-primary.40');
      margin: 0;
    }

    .detail-row {
      display: flex;
      align-items: flex-start;
      gap: 16px;
      padding: 8px 0;
    }

    .detail-icon {
      color: theme('colors.blue-primary.50');
      font-size: 24px;
      width: 24px;
      height: 24px;
      flex-shrink: 0;
    }

    .detail-content {
      display: flex;
      flex-direction: column;
      gap: 4px;
      flex: 1;
    }

    .detail-label {
      font-size: 0.875rem;
      color: theme('colors.blue-neutral.40');
      font-weight: 500;
    }

    .detail-value {
      font-size: 1rem;
      color: theme('colors.blue-neutral.20');
    }

    .category-chip,
    .subcategory-chip {
      width: fit-content;
    }

    mat-divider {
      margin: 8px 0;
    }

    @media (max-width: 768px) {
      .dialog-content {
        min-width: unset;
        width: 100%;
      }

      .dialog-title {
        font-size: 1.25rem;
      }

      .section-title {
        font-size: 1rem;
      }

      .detail-row {
        gap: 12px;
      }
    }
  `]
})
export class ContactDetailsDialog implements OnInit {
  contact = signal<Contact | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  constructor(
    private readonly dialogRef: MatDialogRef<ContactDetailsDialog>,
    private readonly contactService: ContactService,
    @Inject(MAT_DIALOG_DATA) private readonly data: {contactId: number}
  ) {}

  ngOnInit(): void {
    this.loadContactDetails();
  }

  private loadContactDetails(): void {
    this.loading.set(true);
    this.error.set(null);

    this.contactService.getContactById(this.data.contactId).subscribe({
      next: (contact) => {
        this.contact.set(contact);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Nie udało się załadować szczegółów kontaktu.');
        this.loading.set(false);
      }
    });
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
