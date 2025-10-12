export interface PagedResult<T> {
  items: T[];
  totalPages: number;
  totalItemsCount: number;
  itemsFrom: number;
  itemsTo: number;
}