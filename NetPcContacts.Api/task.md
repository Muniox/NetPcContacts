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
		• w przypadku wybrania opcji „służbowy” powinna być możliwość wybrania podkategorii ze słownika (np. szef, klient, itp.), a w przypadku opcji „inny” możliwość wpisania dowolnej podkategorii,
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
	• Ograniczenia znaków w bazie danych (EF Configuration)
	• poprawienie zwracanych błędów przez custom exceptions - done
	• walidacja danych wejściowych - done
	• paginacja (searchable, sortable) - done
	• Logowanie użytkownika w global exceptions - done
	• testy jednostkowe
	• rate limiting - done
	• caching
	• dodać do OpenApi zwracane Statusy z opisami - done