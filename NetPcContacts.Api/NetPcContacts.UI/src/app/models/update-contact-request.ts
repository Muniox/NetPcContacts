export interface UpdateContactRequest {
  name: string;
  surname: string;
  email: string;
  password?: string | null; // Optional for updates
  phoneNumber: string;
  birthDate: string; // ISO date string (YYYY-MM-DD)
  categoryId: number;
  subcategoryId?: number | null;
  customSubcategory?: string | null;
}
