import {Component, inject, signal, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {MatDialogModule, MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatNativeDateModule} from '@angular/material/core';
import {MatSelectModule} from '@angular/material/select';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';

import {ContactService} from '../../services/contact.service';
import {Contact, CreateContactRequest, UpdateContactRequest} from '../../models';

@Component({
  selector: 'app-contact-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatTooltipModule
  ],
  template: `
    <h2 mat-dialog-title>{{ isEditMode() ? 'Edytuj Kontakt' : 'Dodaj Nowy Kontakt' }}</h2>

    <form [formGroup]="contactForm" (ngSubmit)="onSubmit()">
      <mat-dialog-content class="dialog-content">
        <div class="form-grid">
          <!-- Name -->
          <mat-form-field appearance="outline">
            <mat-label>Imię</mat-label>
            <input matInput formControlName="name" required>
            @if (contactForm.get('name')?.hasError('required') && contactForm.get('name')?.touched) {
              <mat-error>Imię jest wymagane</mat-error>
            }
            @if (contactForm.get('name')?.hasError('maxlength')) {
              <mat-error>Imię może mieć maksymalnie 100 znaków</mat-error>
            }
          </mat-form-field>

          <!-- Surname -->
          <mat-form-field appearance="outline">
            <mat-label>Nazwisko</mat-label>
            <input matInput formControlName="surname" required>
            @if (contactForm.get('surname')?.hasError('required') && contactForm.get('surname')?.touched) {
              <mat-error>Nazwisko jest wymagane</mat-error>
            }
            @if (contactForm.get('surname')?.hasError('maxlength')) {
              <mat-error>Nazwisko może mieć maksymalnie 100 znaków</mat-error>
            }
          </mat-form-field>

          <!-- Email -->
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" required>
            <mat-icon matPrefix>email</mat-icon>
            @if (contactForm.get('email')?.hasError('required') && contactForm.get('email')?.touched) {
              <mat-error>Email jest wymagany</mat-error>
            }
            @if (contactForm.get('email')?.hasError('email')) {
              <mat-error>Nieprawidłowy format email</mat-error>
            }
          </mat-form-field>

          <!-- Password -->
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Hasło{{ isEditMode() ? ' (zostaw puste aby nie zmieniać)' : '' }}</mat-label>
            <input matInput [type]="hidePassword() ? 'password' : 'text'" formControlName="password" [required]="!isEditMode()">
            <mat-icon matPrefix>lock</mat-icon>
            <button mat-icon-button matSuffix type="button" (click)="togglePasswordVisibility()" [matTooltip]="hidePassword() ? 'Pokaż hasło' : 'Ukryj hasło'">
              <mat-icon>{{ hidePassword() ? 'visibility_off' : 'visibility' }}</mat-icon>
            </button>
            @if (contactForm.get('password')?.hasError('required') && contactForm.get('password')?.touched) {
              <mat-error>Hasło jest wymagane</mat-error>
            }
            @if (contactForm.get('password')?.hasError('minlength')) {
              <mat-error>Hasło musi mieć minimum 8 znaków</mat-error>
            }
          </mat-form-field>

          <!-- Phone Number -->
          <mat-form-field appearance="outline">
            <mat-label>Numer telefonu</mat-label>
            <input matInput formControlName="phoneNumber" required>
            <mat-icon matPrefix>phone</mat-icon>
            @if (contactForm.get('phoneNumber')?.hasError('required') && contactForm.get('phoneNumber')?.touched) {
              <mat-error>Numer telefonu jest wymagany</mat-error>
            }
            @if (contactForm.get('phoneNumber')?.hasError('pattern')) {
              <mat-error>Numer telefonu musi mieć 9-20 cyfr</mat-error>
            }
          </mat-form-field>

          <!-- Birth Date -->
          <mat-form-field appearance="outline">
            <mat-label>Data urodzenia</mat-label>
            <input matInput [matDatepicker]="picker" formControlName="birthDate" required>
            <mat-datepicker-toggle matIconSuffix [for]="picker"></mat-datepicker-toggle>
            <mat-datepicker #picker></mat-datepicker>
            @if (contactForm.get('birthDate')?.hasError('required') && contactForm.get('birthDate')?.touched) {
              <mat-error>Data urodzenia jest wymagana</mat-error>
            }
          </mat-form-field>

          <!-- Category -->
          <mat-form-field appearance="outline">
            <mat-label>Kategoria</mat-label>
            <mat-select formControlName="categoryId" required>
              <mat-option [value]="1">Służbowy</mat-option>
              <mat-option [value]="2">Prywatny</mat-option>
              <mat-option [value]="3">Inny</mat-option>
            </mat-select>
            @if (contactForm.get('categoryId')?.hasError('required') && contactForm.get('categoryId')?.touched) {
              <mat-error>Kategoria jest wymagana</mat-error>
            }
          </mat-form-field>

          <!-- Subcategory (for Business) -->
          @if (contactForm.get('categoryId')?.value === 1) {
            <mat-form-field appearance="outline">
              <mat-label>Podkategoria *</mat-label>
              <mat-select formControlName="subcategoryId" required>
                <mat-option [value]="1">szef</mat-option>
                <mat-option [value]="2">współpracownik</mat-option>
                <mat-option [value]="3">klient</mat-option>
              </mat-select>
              @if (contactForm.get('subcategoryId')?.hasError('required') && contactForm.get('subcategoryId')?.touched) {
                <mat-error>Podkategoria jest wymagana dla kontaktów służbowych</mat-error>
              }
            </mat-form-field>
          }

          <!-- Custom Subcategory (for Other) -->
          @if (contactForm.get('categoryId')?.value === 3) {
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Własna podkategoria *</mat-label>
              <input matInput formControlName="customSubcategory" maxlength="100" required>
              @if (contactForm.get('customSubcategory')?.hasError('required') && contactForm.get('customSubcategory')?.touched) {
                <mat-error>Własna podkategoria jest wymagana</mat-error>
              }
              @if (contactForm.get('customSubcategory')?.hasError('maxlength')) {
                <mat-error>Maksymalnie 100 znaków</mat-error>
              }
            </mat-form-field>
          }
        </div>

        @if (errorMessage()) {
          <div class="error-message">
            <mat-icon>error</mat-icon>
            {{ errorMessage() }}
          </div>
        }
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-stroked-button type="button" (click)="onCancel()" [disabled]="isLoading()">
          Anuluj
        </button>
        <button mat-stroked-button color="primary" type="submit" [disabled]="contactForm.invalid || isLoading()">
          @if (isLoading()) {
            <mat-spinner diameter="20"></mat-spinner>
          } @else {
            {{ isEditMode() ? 'Zapisz' : 'Dodaj' }}
          }
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .dialog-content {
      min-width: 500px;
      max-width: 600px;
      padding: 20px 24px;
    }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .full-width {
      grid-column: 1 / -1;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px;
      margin-top: 16px;
      background-color: #ffebee;
      border-radius: 4px;
      color: #c62828;
    }

    .error-message mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    mat-dialog-actions button {
      min-width: 100px;
    }

    mat-spinner {
      display: inline-block;
    }

    @media (max-width: 768px) {
      .dialog-content {
        min-width: unset;
        width: 100%;
      }

      .form-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ContactDialog implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<ContactDialog>);
  private readonly fb = inject(FormBuilder);
  private readonly contactService = inject(ContactService);
  readonly data = inject<{ contactId?: number }>(MAT_DIALOG_DATA, { optional: true });

  contactForm: FormGroup;
  hidePassword = signal(true);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  isEditMode = signal(false);
  contactId: number | null = null;

  constructor() {
    this.contactForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      surname: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{9,20}$/)]],
      birthDate: ['', Validators.required],
      categoryId: [null, Validators.required],
      subcategoryId: [null],
      customSubcategory: ['']
    });

    // Watch category changes to update validators dynamically
    this.contactForm.get('categoryId')?.valueChanges.subscribe((categoryId) => {
      const subcategoryIdControl = this.contactForm.get('subcategoryId');
      const customSubcategoryControl = this.contactForm.get('customSubcategory');

      // Reset values
      this.contactForm.patchValue({
        subcategoryId: null,
        customSubcategory: ''
      });

      // Clear all validators first
      subcategoryIdControl?.clearValidators();
      customSubcategoryControl?.clearValidators();

      // Category 1 = "Służbowy" - subcategoryId is REQUIRED
      if (categoryId === 1) {
        subcategoryIdControl?.setValidators([Validators.required]);
      }
      // Category 3 = "Inny" - customSubcategory is REQUIRED
      else if (categoryId === 3) {
        customSubcategoryControl?.setValidators([Validators.required, Validators.maxLength(100)]);
      }

      // Update validity
      subcategoryIdControl?.updateValueAndValidity();
      customSubcategoryControl?.updateValueAndValidity();
    });
  }

  ngOnInit(): void {
    // Check if we're in edit mode
    if (this.data?.contactId) {
      this.isEditMode.set(true);
      this.contactId = this.data.contactId;
      this.loadContact(this.data.contactId);

      // Make password optional for edit mode
      const passwordControl = this.contactForm.get('password');
      passwordControl?.clearValidators();
      passwordControl?.setValidators([Validators.minLength(8)]);
      passwordControl?.updateValueAndValidity();
    }
  }

  loadContact(contactId: number): void {
    this.isLoading.set(true);
    this.contactService.getContactById(contactId).subscribe({
      next: (contact: Contact) => {
        // Parse date string to Date object
        const birthDate = new Date(contact.birthDate);

        this.contactForm.patchValue({
          name: contact.name,
          surname: contact.surname,
          email: contact.email,
          phoneNumber: contact.phoneNumber,
          birthDate: birthDate,
          categoryId: contact.categoryId,
          subcategoryId: contact.subcategoryId,
          customSubcategory: contact.customSubcategory
        });
        this.isLoading.set(false);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set('Nie udało się załadować danych kontaktu.');
      }
    });
  }

  togglePasswordVisibility(): void {
    this.hidePassword.set(!this.hidePassword());
  }

  onSubmit(): void {
    if (this.contactForm.invalid) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const formValue = this.contactForm.value;

    if (this.isEditMode() && this.contactId) {
      // Update existing contact
      const request: UpdateContactRequest = {
        name: formValue.name,
        surname: formValue.surname,
        email: formValue.email,
        password: formValue.password || null, // null if empty
        phoneNumber: formValue.phoneNumber,
        birthDate: this.formatDate(formValue.birthDate),
        categoryId: formValue.categoryId,
        subcategoryId: formValue.categoryId === 1 ? formValue.subcategoryId : null,
        customSubcategory: formValue.categoryId === 3 ? formValue.customSubcategory : null
      };

      this.contactService.updateContact(this.contactId, request).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.dialogRef.close(true);
        },
        error: (error) => {
          this.isLoading.set(false);
          console.error('Update contact error:', error);
          console.error('Request payload:', request);

          let errorMsg = 'Nie udało się zaktualizować kontaktu. Spróbuj ponownie.';
          if (error.error?.errors) {
            const validationErrors = Object.values(error.error.errors).flat();
            errorMsg = validationErrors.join(' ');
          } else if (error.error?.message) {
            errorMsg = error.error.message;
          }

          this.errorMessage.set(errorMsg);
        }
      });
    } else {
      // Create new contact
      const request: CreateContactRequest = {
        name: formValue.name,
        surname: formValue.surname,
        email: formValue.email,
        password: formValue.password,
        phoneNumber: formValue.phoneNumber,
        birthDate: this.formatDate(formValue.birthDate),
        categoryId: formValue.categoryId,
        subcategoryId: formValue.categoryId === 1 ? formValue.subcategoryId : null,
        customSubcategory: formValue.categoryId === 3 ? formValue.customSubcategory : null
      };

      this.contactService.createContact(request).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.dialogRef.close(true);
        },
        error: (error) => {
          this.isLoading.set(false);
          console.error('Create contact error:', error);
          console.error('Request payload:', request);

          let errorMsg = 'Nie udało się dodać kontaktu. Spróbuj ponownie.';
          if (error.error?.errors) {
            const validationErrors = Object.values(error.error.errors).flat();
            errorMsg = validationErrors.join(' ');
          } else if (error.error?.message) {
            errorMsg = error.error.message;
          }

          this.errorMessage.set(errorMsg);
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
