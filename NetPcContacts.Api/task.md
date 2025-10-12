 # Funkcjonalności:
## 1. Logowanie:
	Funkcjonalność z pkt. 2 dostępna jest dla niezalogowanego użytkownika, pozostałe wymagają zalogowania.
## 2. Przeglądanie listy kontaktów:
	Lista powinna zawierać dane podstawowe. Po wybraniu konkretnego kontaktu wyświetlane są jego
	szczegóły.
## 3. Szczegóły kontaktu:
	Zalogowany użytkownik może edytować i usuwać istniejące wpisy oraz dodawać nowe. Pojedynczy kontakt
	powinien mieć przynajmniej:
		• imię,
		• nazwisko,
		• email – unikalny,
		• hasło – powinno spełniać podstawowe standardy złożoności hasła,
		• kategoria (służbowy, prywatny, inny),
		• w przypadku wybrania opcji „służbowy" powinna być możliwość wybrania podkategorii ze słownika (np. szef, klient, itp.), a w przypadku opcji „inny" możliwość wpisania dowolnej podkategorii,
		• telefon,
		• data urodzenia.
	Założenia techniczne:
		• Aplikacja powinna być napisana w języku C# z użyciem dowolnej bazy danych.
		• Architektura aplikacji backendowej - REST API - wymagane
		• Architektura aplikacji frontendowej - Single Page Application - wymagane
		• Wszelkie dane słownikowe (kategorie, podkategorie) powinny być trzymane w bazie danych - wymagane
		• Zaleca się wykorzystanie darmowych bibliotek
		• Należy zwrócić uwagę na bezpieczeństwo aplikacji
		• Kod źródłowy powinien zawierać komentarze
		• Wygląd graficzny aplikacji jest nieistotny
	Do zadania należy przygotować krótką specyfikację techniczną zawierającą:
		• opis poszczególnych klas i metod,
		• wykorzystane biblioteki,
		• sposób kompilacji aplikacji.

## Dodatkowo
	• Ograniczenia znaków w bazie danych (EF Configuration) - done
	• poprawienie zwracanych błędów przez custom exceptions - done
	• walidacja danych wejściowych - done
	• paginacja (searchable, sortable) - done
	• Logowanie użytkownika w global exceptions - done
	• testy jednostkowe - done
	• rate limiting - done
	• caching
	• dodać do OpenApi zwracane Statusy z opisami - done

---

## Reguły walidacji (FluentValidation)

### CreateContactCommandValidator

#### Name (Imię)
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `NotEmpty()` | Zawsze | "Imię jest wymagane." |
| `Length(1, 100)` | Zawsze | "Imię musi zawierać od 1 do 100 znaków." |

#### Surname (Nazwisko)
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `NotEmpty()` | Zawsze | "Nazwisko jest wymagane." |
| `Length(1, 100)` | Zawsze | "Nazwisko musi zawierać od 1 do 100 znaków." |

#### Email
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `NotEmpty()` | Zawsze | "Email jest wymagany." |
| `EmailAddress()` | Zawsze | "Nieprawidłowy format adresu email." |
| `MaximumLength(255)` | Zawsze | "Email nie może przekraczać 255 znaków." |

**Uwaga:** Unikalność emaila sprawdzana w `CreateContactCommandHandler` przez `EmailExists()`.

#### Password (Hasło)
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `NotEmpty()` | Zawsze | "Hasło jest wymagane." |
| `MinimumLength(8)` | Zawsze | "Hasło musi zawierać minimum 8 znaków." |
| `MaximumLength(100)` | Zawsze | "Hasło nie może przekraczać 100 znaków." |
| `Matches(@"[A-Z]")` | Zawsze | "Hasło musi zawierać przynajmniej jedną wielką literę." |
| `Matches(@"[a-z]")` | Zawsze | "Hasło musi zawierać przynajmniej jedną małą literę." |
| `Matches(@"[0-9]")` | Zawsze | "Hasło musi zawierać przynajmniej jedną cyfrę." |
| `Matches(@"[\W_]")` | Zawsze | "Hasło musi zawierać przynajmniej jeden znak specjalny." |

**Wymagania złożoności hasła:**
- Min. 8 znaków
- Max. 100 znaków
- Co najmniej jedna wielka litera (A-Z)
- Co najmniej jedna mała litera (a-z)
- Co najmniej jedna cyfra (0-9)
- Co najmniej jeden znak specjalny (!@#$%^&* itp.)

#### PhoneNumber (Numer telefonu)
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `NotEmpty()` | Zawsze | "Numer telefonu jest wymagany." |
| `Matches(@"^[\d\s\-\+\(\)]+$")` | Zawsze | "Numer telefonu może zawierać tylko cyfry, spacje, myślniki, plus i nawiasy." |
| `Length(9, 20)` | Zawsze | "Numer telefonu musi zawierać od 9 do 20 znaków." |

**Dozwolone znaki:** cyfry (0-9), spacje, myślnik (-), plus (+), nawiasy ()

#### BirthDate (Data urodzenia)
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `NotEmpty()` | Zawsze | "Data urodzenia jest wymagana." |
| `LessThan(DateOnly.FromDateTime(DateTime.Today))` | Zawsze | "Data urodzenia musi być datą z przeszłości." |
| `GreaterThan(DateOnly.FromDateTime(DateTime.Today.AddYears(-150)))` | Zawsze | "Data urodzenia nie może być starsza niż 150 lat." |

**Ograniczenia:** Data musi być z przeszłości, nie starsza niż 150 lat od dzisiaj.

#### CategoryId
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| Sprawdzenie w handlerze | - | Weryfikacja istnienia kategorii w bazie |

#### SubcategoryId (opcjonalne)
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `GreaterThan(0)` | `When(x => x.SubcategoryId.HasValue)` | "Podkategoria musi mieć prawidłową wartość." |

**Walidacja w handlerze:** Sprawdzenie czy SubcategoryId należy do CategoryId.

#### CustomSubcategory (opcjonalne)
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `NotEmpty()` | `When(x => !string.IsNullOrWhiteSpace(x.CustomSubcategory))` | "Niestandardowa podkategoria nie może być pusta." |
| `MaximumLength(100)` | `When(x => !string.IsNullOrWhiteSpace(x.CustomSubcategory))` | "Niestandardowa podkategoria nie może przekraczać 100 znaków." |

---

### UpdateContactCommandValidator

Reguły identyczne jak w `CreateContactCommandValidator` z następującymi różnicami:

#### Id
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `GreaterThan(0)` | Zawsze | "ID kontaktu musi być większe od zera." |

#### Password (opcjonalne przy update)
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `MinimumLength(8)` | `When(x => !string.IsNullOrWhiteSpace(x.Password))` | "Hasło musi zawierać minimum 8 znaków." |
| `MaximumLength(100)` | `When(x => !string.IsNullOrWhiteSpace(x.Password))` | "Hasło nie może przekraczać 100 znaków." |
| `Matches(@"[A-Z]")` | `When(x => !string.IsNullOrWhiteSpace(x.Password))` | "Hasło musi zawierać przynajmniej jedną wielką litera." |
| `Matches(@"[a-z]")` | `When(x => !string.IsNullOrWhiteSpace(x.Password))` | "Hasło musi zawierać przynajmniej jedną małą literę." |
| `Matches(@"[0-9]")` | `When(x => !string.IsNullOrWhiteSpace(x.Password))` | "Hasło musi zawierać przynajmniej jedną cyfrę." |
| `Matches(@"[\W_]")` | `When(x => !string.IsNullOrWhiteSpace(x.Password))` | "Hasło musi zawierać przynajmniej jeden znak specjalny." |

**Uwaga:** Przy aktualizacji, jeśli `Password` jest `null` lub pusty, hasło nie jest zmieniane.

---

### GetAllContactsQueryValidator

#### PageNumber
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `GreaterThanOrEqualTo(1)` | Zawsze | Domyślny komunikat FluentValidation |

**Ograniczenie:** Numer strony musi być >= 1.

#### PageSize
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `Must(value => allowPageSizes.Contains(value))` | Zawsze | "Page size must be in [5,10,15,30]" |

**Dozwolone wartości:** 5, 10, 15, 30

#### SortBy (opcjonalne)
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| `Must(value => allowedSortByColumnNames.Contains(value))` | `When(q => q.SortBy != null)` | "Sort by is optional, or must be in [FirstName,LastName,Category]" |

**Dozwolone wartości:** "FirstName", "LastName", "Category"

#### SortDirection
| Typ | Wartości | Opis |
|-----|----------|------|
| `Enum` | `Ascending (0)`, `Descending (1)` | Kierunek sortowania |

#### SearchPhrase (opcjonalne)
| Reguła | Warunek | Komunikat błędu |
|--------|---------|-----------------|
| Brak walidacji | - | Filtruje po Name, Surname, Email |

---

## Ograniczenia długości pól (Walidacja ↔ Baza Danych)

### Encja Contact
| Pole | FluentValidation | Entity Framework | Status |
|------|------------------|------------------|--------|
| **Name** | `Length(1, 100)` | `HasMaxLength(100)` | ✅ Zgodne |
| **Surname** | `Length(1, 100)` | `HasMaxLength(100)` | ✅ Zgodne |
| **Email** | `MaximumLength(255)` | `HasMaxLength(255)` + Unique Index | ✅ Zgodne |
| **Password** | `MinimumLength(8)`, `MaximumLength(100)` | PasswordHash: `HasMaxLength(500)` | ✅ Zgodne* |
| **PhoneNumber** | `Length(9, 20)` | `HasMaxLength(20)` | ✅ Zgodne |
| **CustomSubcategory** | `MaximumLength(100)` | `HasMaxLength(100)` | ✅ Zgodne |

*PasswordHash jest dłuższy (500) niż ograniczenie hasła w plaintext (100), ponieważ hash PBKDF2 z Identity generuje ~100-150 znaków.

### Encja Category
| Pole | Baza Danych | Uzasadnienie |
|------|-------------|--------------|
| **CategoryName** | `HasMaxLength(50)` | Słownik ("Służbowy", "Prywatny", "Inny") |

### Encja Subcategory
| Pole | Baza Danych | Uzasadnienie |
|------|-------------|--------------|
| **SubcategoryName** | `HasMaxLength(100)` | Słownik podkategorii ("Szef", "Klient", "Dostawca") |

---

## Walidacja biznesowa (w Handlerach)

### CreateContactCommandHandler
| Walidacja | Metoda | Wyjątek |
|-----------|--------|---------|
| **Unikalność emaila** | `EmailExists(email)` | `DuplicateEmailException` |
| **Istnienie kategorii** | `CategoryRepository.Exists(categoryId)` | `NotFoundException` |
| **Zgodność podkategorii z kategorią** | `SubcategoryRepository.ExistsForCategory(subcategoryId, categoryId)` | `NotFoundException` |

### UpdateContactCommandHandler
| Walidacja | Metoda | Wyjątek |
|-----------|--------|---------|
| **Istnienie kontaktu** | `GetById(id)` | `NotFoundException` |
| **Unikalność emaila** | `EmailExists(email)` (jeśli zmieniony) | `DuplicateEmailException` |
| **Istnienie kategorii** | `CategoryRepository.Exists(categoryId)` | `NotFoundException` |
| **Zgodność podkategorii z kategorią** | `SubcategoryRepository.ExistsForCategory(subcategoryId, categoryId)` | `NotFoundException` |

### DeleteContactCommandHandler
| Walidacja | Metoda | Wyjątek |
|-----------|--------|---------|
| **Istnienie kontaktu** | `GetById(id)` | `NotFoundException` |

### GetContactByIdQueryHandler
| Walidacja | Metoda | Wyjątek |
|-----------|--------|---------|
| **Istnienie kontaktu** | `GetById(id)` | `NotFoundException` |

---

## Konfiguracja w NetPcContactsDbContext
Wszystkie ograniczenia są zaimplementowane w metodzie `OnModelCreating`:
- `IsRequired()` - pola wymagane
- `HasMaxLength(X)` - maksymalna długość
- `HasIndex().IsUnique()` - indeks unikalny dla emaila
- Relacje z `OnDelete(DeleteBehavior.SetNull)`

---

	