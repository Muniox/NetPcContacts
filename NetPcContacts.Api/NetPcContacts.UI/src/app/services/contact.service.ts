import {computed, inject, Injectable, signal} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable, tap} from 'rxjs';

import {environment} from '../../environments/environment';
import {BasicContact, Contact, ContactQuery, PagedResult, SortDirection} from '../models';

@Injectable({
  providedIn: 'root'
})
export class ContactService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/contact`;

  // Signals for state management
  private readonly contactsSignal = signal<PagedResult<BasicContact> | null>(null);
  private readonly loadingSignal = signal<boolean>(false);
  private readonly errorSignal = signal<string | null>(null);

  // Query parameters signals
  private readonly searchPhraseSignal = signal<string>('');
  private readonly pageNumberSignal = signal<number>(1);
  private readonly pageSizeSignal = signal<number>(10);
  private readonly sortBySignal = signal<'Name' | 'Surname' | 'Category' | undefined>('Name');
  private readonly sortDirectionSignal = signal<SortDirection>(SortDirection.Ascending);

  // Public computed signals
  readonly contacts = computed(() => this.contactsSignal());
  readonly loading = computed(() => this.loadingSignal());
  readonly error = computed(() => this.errorSignal());
  readonly searchPhrase = computed(() => this.searchPhraseSignal());
  readonly pageNumber = computed(() => this.pageNumberSignal());
  readonly pageSize = computed(() => this.pageSizeSignal());
  readonly sortBy = computed(() => this.sortBySignal());
  readonly sortDirection = computed(() => this.sortDirectionSignal());

  // Computed for pagination info
  readonly hasNextPage = computed(() => {
    const contacts = this.contactsSignal();
    return contacts ? this.pageNumberSignal() < contacts.totalPages : false;
  });

  readonly hasPreviousPage = computed(() => this.pageNumberSignal() > 1);

  getAllContacts(query: ContactQuery): Observable<PagedResult<BasicContact>> {
    this.loadingSignal.set(true);
    this.errorSignal.set(null);

    let params = new HttpParams()
      .set('pageNumber', query.pageNumber.toString())
      .set('pageSize', query.pageSize.toString())
      .set('sortDirection', query.sortDirection.toString());

    if (query.searchPhrase) {
      params = params.set('searchPhrase', query.searchPhrase);
    }

    if (query.sortBy) {
      params = params.set('sortBy', query.sortBy);
    }

    return this.http.get<PagedResult<BasicContact>>(this.apiUrl, {params})
      .pipe(
        tap({
          next: (result) => {
            this.contactsSignal.set(result);
            this.loadingSignal.set(false);
          },
          error: (error) => {
            this.errorSignal.set(error.message || 'Failed to load contacts');
            this.loadingSignal.set(false);
          }
        })
      );
  }

  getContactById(id: number): Observable<Contact> {
    return this.http.get<Contact>(`${this.apiUrl}/${id}`);
  }

  // Helper methods to update query parameters
  setSearchPhrase(searchPhrase: string): void {
    this.searchPhraseSignal.set(searchPhrase);
    this.pageNumberSignal.set(1); // Reset to first page on search
  }

  setPageNumber(pageNumber: number): void {
    this.pageNumberSignal.set(pageNumber);
  }

  setPageSize(pageSize: number): void {
    this.pageSizeSignal.set(pageSize);
    this.pageNumberSignal.set(1); // Reset to first page on page size change
  }

  setSortBy(sortBy: 'Name' | 'Surname' | 'Category' | undefined): void {
    this.sortBySignal.set(sortBy);
  }

  setSortDirection(sortDirection: SortDirection): void {
    this.sortDirectionSignal.set(sortDirection);
  }

  // Method to load contacts with current query parameters
  loadContacts(): void {
    const query: ContactQuery = {
      searchPhrase: this.searchPhraseSignal(),
      pageNumber: this.pageNumberSignal(),
      pageSize: this.pageSizeSignal(),
      sortBy: this.sortBySignal(),
      sortDirection: this.sortDirectionSignal()
    };

    this.getAllContacts(query).subscribe();
  }

  nextPage(): void {
    if (this.hasNextPage()) {
      this.setPageNumber(this.pageNumberSignal() + 1);
      this.loadContacts();
    }
  }

  previousPage(): void {
    if (this.hasPreviousPage()) {
      this.setPageNumber(this.pageNumberSignal() - 1);
      this.loadContacts();
    }
  }
}