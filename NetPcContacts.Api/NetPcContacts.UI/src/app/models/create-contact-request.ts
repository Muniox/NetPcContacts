export interface CreateContactRequest {
  name: string;
  surname: string;
  email: string;
  password: string;
  phoneNumber: string;
  birthDate: string; // ISO date string (YYYY-MM-DD)
  categoryId: number;
  subcategoryId?: number | null;
  customSubcategory?: string | null;
}
