export enum SortDirection {
  Ascending = 0,
  Descending = 1
}

export interface ContactQuery {
  searchPhrase?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: 'Name' | 'Surname' | 'Category';
  sortDirection: SortDirection;
}