# Naprawa logiki Refresh Token

## Problem

Refresh token nie był odświeżany na front-endzie, mimo że backend działał poprawnie. Po przeładowaniu strony użytkownik był wylogowywany, a automatyczne odświeżanie tokena przy wygaśnięciu access tokena nie działało.

## Zidentyfikowane problemy

### 1. Nieprawidłowy zapis refresh tokena do cookies
**Lokalizacja:** `NetPcContacts.Api/NetPcContacts.UI/src/app/services/auth-service.ts:39-41`

**Problem:**
```typescript
// Przed naprawą
document.cookie = `refreshToken=${token}`;  // Sesyjne ciasteczko bez atrybutów
document.cookie = 'refreshToken=;';         // Niepoprawne usuwanie
```

**Konsekwencje:**
- Ciasteczko bez atrybutu `expires`/`max-age` było **sesyjne** i znikało po zamknięciu przeglądarki
- Brak atrybutów bezpieczeństwa (`Secure`, `SameSite`, `Path`)
- Refresh token nie przetrwał przeładowania strony
- Niepoprawne usuwanie ciasteczka (brak daty wygaśnięcia w przeszłości)

### 2. Brak `untracked()` w efektach
**Lokalizacja:** `NetPcContacts.Api/NetPcContacts.UI/src/app/services/auth-service.ts:26-43`

**Problem:**
```typescript
// Przed naprawą
effect(() => {
  const token = this.accessTokenSignal();
  // Bezpośrednie operacje na localStorage/cookies
  localStorage.setItem('accessToken', token);
});
```

**Konsekwencje:**
- Zgodnie z Angular best practices (CLAUDE.md), side effects powinny być w `untracked()`
- Potencjalne reaktywne pętle
- Niezgodność z wzorcem Signal Effects Pattern

### 3. Niekonsekwentne użycie signali
**Lokalizacja:**
- `NetPcContacts.Api/NetPcContacts.UI/src/app/interceptors/jwt-interceptor.ts:13`
- `NetPcContacts.Api/NetPcContacts.UI/src/app/interceptors/error-interceptor.ts:29`

**Problem:**
```typescript
// Przed naprawą
const accessToken = localStorage.getItem('accessToken');  // Bezpośredni dostęp
```

**Konsekwencje:**
- Interceptory omijały system reaktywności Angular
- Brak konsystencji z architekturą opartą na signalach
- Potencjalne problemy z synchronizacją stanu

## Wprowadzone zmiany

### Zmiana 1: Dodanie metod pomocniczych do zarządzania cookies

**Plik:** `NetPcContacts.Api/NetPcContacts.UI/src/app/services/auth-service.ts`
**Linie:** 100-111

**Dodany kod:**
```typescript
private setCookie(name: string, value: string, days: number): void {
  const date = new Date();
  date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
  const expires = `expires=${date.toUTCString()}`;
  const secure = window.location.protocol === 'https:' ? 'Secure;' : '';
  document.cookie = `${name}=${value};${expires};path=/;${secure}SameSite=Strict`;
}

private deleteCookie(name: string): void {
  const secure = window.location.protocol === 'https:' ? 'Secure;' : '';
  document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;${secure}SameSite=Strict`;
}
```

**Opis:**
- `setCookie()` - tworzy ciasteczko z prawidłowymi atrybutami:
  - `expires` - data wygaśnięcia (parametr `days` określa liczbę dni ważności)
  - `path=/` - ciasteczko dostępne dla całej aplikacji
  - `Secure` - ciasteczko wysyłane tylko przez HTTPS (jeśli aplikacja działa na HTTPS)
  - `SameSite=Strict` - ochrona przed atakami CSRF
- `deleteCookie()` - poprawnie usuwa ciasteczko przez ustawienie daty wygaśnięcia w przeszłości

**Dlaczego to rozwiązuje problem:**
Refresh token jest teraz zapisywany jako **persistent cookie** z 7-dniowym czasem ważności, co pozwala mu przetrwać przeładowanie strony i zamknięcie przeglądarki.

### Zmiana 2: Dodanie importu `untracked`

**Plik:** `NetPcContacts.Api/NetPcContacts.UI/src/app/services/auth-service.ts`
**Linia:** 1

**Zmiana:**
```typescript
// Przed
import {computed, effect, inject, Injectable, signal} from '@angular/core';

// Po
import {computed, effect, inject, Injectable, signal, untracked} from '@angular/core';
```

**Opis:**
Dodanie `untracked` do importów Angular Core, aby móc używać go w efektach.

### Zmiana 3: Poprawienie efektów w konstruktorze

**Plik:** `NetPcContacts.Api/NetPcContacts.UI/src/app/services/auth-service.ts`
**Linie:** 24-48

**Zmiana:**
```typescript
// Przed
constructor() {
  effect(() => {
    const token = this.accessTokenSignal();
    if (token) {
      localStorage.setItem('accessToken', token);
    } else {
      localStorage.removeItem('accessToken');
    }
  });

  effect(() => {
    const token = this.refreshTokenSignal();
    if (token) {
      document.cookie = `refreshToken=${token}`;  // ❌ Nieprawidłowy zapis
    } else {
      document.cookie = 'refreshToken=;';         // ❌ Nieprawidłowe usuwanie
    }
  });
}

// Po
constructor() {
  // Sync accessToken changes to localStorage
  effect(() => {
    const token = this.accessTokenSignal();
    untracked(() => {
      if (token) {
        localStorage.setItem('accessToken', token);
      } else {
        localStorage.removeItem('accessToken');
      }
    });
  });

  // Sync refreshToken changes to cookie (7 days expiration)
  effect(() => {
    const token = this.refreshTokenSignal();
    untracked(() => {
      if (token) {
        this.setCookie('refreshToken', token, 7);  // ✅ Prawidłowy zapis z 7-dniową ważnością
      } else {
        this.deleteCookie('refreshToken');         // ✅ Prawidłowe usuwanie
      }
    });
  });
}
```

**Opis:**
- Opakowano operacje localStorage i cookies w `untracked()` zgodnie z Angular Signal Effects Pattern
- Użyto nowych metod `setCookie()` i `deleteCookie()` zamiast bezpośredniej manipulacji `document.cookie`
- Refresh token jest teraz zapisywany z 7-dniowym czasem ważności

**Dlaczego to rozwiązuje problem:**
- `untracked()` zapobiega reaktywnym pętlom
- Proper cookie attributes zapewniają, że refresh token przetrwa sesję przeglądarki
- Zgodność z Angular best practices

### Zmiana 4: Poprawienie JWT Interceptor

**Plik:** `NetPcContacts.Api/NetPcContacts.UI/src/app/interceptors/jwt-interceptor.ts`
**Linie:** 6-19

**Zmiana:**
```typescript
// Przed
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const isLoggedIn = authService.isLoggedIn();

  if (isLoggedIn) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${localStorage.getItem('accessToken')}`  // ❌ Bezpośredni dostęp
      }
    })
  }

  return next(req);
};

// Po
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const accessToken = authService.accessToken();  // ✅ Użycie signala

  if (accessToken) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`
      }
    })
  }

  return next(req);
};
```

**Opis:**
- Zamiast bezpośredniego dostępu do `localStorage`, używamy `authService.accessToken()` (computed signal)
- Sprawdzamy bezpośrednio czy `accessToken` istnieje, zamiast używać `isLoggedIn()`

**Dlaczego to rozwiązuje problem:**
- Konsystencja z architekturą opartą na signalach
- Lepsze zarządzanie stanem i reaktywność
- Jeden source of truth dla access tokena

### Zmiana 5: Poprawienie Error Interceptor

**Plik:** `NetPcContacts.Api/NetPcContacts.UI/src/app/interceptors/error-interceptor.ts`
**Linie:** 28-44

**Zmiana:**
```typescript
// Przed
const addToken = (req: HttpRequest<unknown>) => {
  const accessToken = localStorage.getItem('accessToken');  // ❌ Bezpośredni dostęp
  if (accessToken) {
    return req.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`
      }
    })
  }
  return req;
}

const handle401Error = (req: HttpRequest<unknown>, next: HttpHandlerFn, authService: AuthService): Observable<any> => {
  return authService.refreshToken()
    .pipe(
      switchMap(() => {
        return next(addToken(req))  // ❌ Brak dostępu do authService
      }),
      // ...
    )
}

// Po
const addToken = (req: HttpRequest<unknown>, authService: AuthService) => {  // ✅ Dodano parametr
  const accessToken = authService.accessToken();  // ✅ Użycie signala
  if (accessToken) {
    return req.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`
      }
    })
  }
  return req;
}

const handle401Error = (req: HttpRequest<unknown>, next: HttpHandlerFn, authService: AuthService): Observable<any> => {
  return authService.refreshToken()
    .pipe(
      switchMap(() => {
        return next(addToken(req, authService))  // ✅ Przekazanie authService
      }),
      // ...
    )
}
```

**Opis:**
- Dodano parametr `authService` do funkcji `addToken()`
- Użyto `authService.accessToken()` zamiast bezpośredniego dostępu do localStorage
- Przekazywanie `authService` do `addToken()` w `handle401Error()`

**Dlaczego to rozwiązuje problem:**
- Po automatycznym odświeżeniu tokena (refresh), nowy access token jest pobierany przez signal
- Konsystencja z architekturą opartą na signalach
- Zapewnia, że po odświeżeniu tokena nowe zapytanie używa aktualnego tokena

## Jak to działa teraz

### Przepływ logowania:
1. Użytkownik loguje się → `login()` wysyła zapytanie do backendu
2. Backend zwraca `accessToken` i `refreshToken`
3. AuthService ustawia sygnały: `accessTokenSignal.set()` i `refreshTokenSignal.set()`
4. Efekty w konstruktorze reagują:
   - `accessToken` → zapisywany do `localStorage`
   - `refreshToken` → zapisywany do **persistent cookie** z 7-dniową ważnością
5. Użytkownik jest zalogowany

### Przepływ automatycznego odświeżania tokena:
1. Access token wygasa (1 minuta)
2. Użytkownik próbuje wykonać zapytanie wymagające autoryzacji
3. Backend zwraca 401 Unauthorized
4. `errorInterceptor` przechwytuje błąd 401
5. Wywołuje `authService.refreshToken()`:
   - Wysyła refresh token do backendu (`/api/identity/refresh`)
   - Backend zwraca nowy `accessToken` i nowy `refreshToken`
6. AuthService aktualizuje sygnały → efekty zapisują nowe tokeny
7. Interceptor ponawia oryginalne zapytanie z nowym tokenem
8. Zapytanie kończy się sukcesem

### Przepływ przeładowania strony:
1. Użytkownik przeładowuje stronę
2. Konstruktor AuthService inicjalizuje sygnały:
   - `accessTokenSignal` z wartością z `localStorage`
   - `refreshTokenSignal` z wartością z **persistent cookie**
3. Jeśli oba tokeny istnieją → użytkownik pozostaje zalogowany
4. Jeśli access token wygasł, ale refresh token jest ważny → automatyczne odświeżenie przy następnym zapytaniu

## Instrukcje testowania

### Test 1: Podstawowe logowanie i przeładowanie strony
1. Otwórz aplikację w przeglądarce
2. Otwórz DevTools → Application → Cookies
3. Zaloguj się do aplikacji
4. Sprawdź czy ciasteczko `refreshToken` ma:
   - ✅ Datę wygaśnięcia (7 dni od teraz)
   - ✅ Atrybut `Path=/`
   - ✅ Atrybut `SameSite=Strict`
   - ✅ Atrybut `Secure` (jeśli HTTPS)
5. Przeładuj stronę (F5)
6. **Oczekiwany rezultat:** Użytkownik pozostaje zalogowany

### Test 2: Automatyczne odświeżanie tokena
1. Zaloguj się do aplikacji
2. Poczekaj 1 minutę (access token wygasa)
3. Wykonaj operację wymagającą autoryzacji (np. utwórz/edytuj kontakt)
4. Otwórz DevTools → Network → sprawdź zapytania:
   - Powinno być zapytanie do `/api/identity/refresh`
   - Po nim powinno być ponowione oryginalne zapytanie
5. **Oczekiwany rezultat:**
   - Token odświeżony automatycznie
   - Operacja zakończona sukcesem
   - Użytkownik nie został wylogowany

### Test 3: Zamknięcie i ponowne otwarcie przeglądarki
1. Zaloguj się do aplikacji
2. Zamknij przeglądarkę całkowicie
3. Otwórz przeglądarkę ponownie
4. Przejdź do aplikacji
5. **Oczekiwany rezultat:** Użytkownik pozostaje zalogowany (refresh token przetrwał)

### Test 4: Wylogowanie
1. Zaloguj się do aplikacji
2. Wyloguj się
3. Sprawdź DevTools → Application → Cookies
4. **Oczekiwany rezultat:** Ciasteczko `refreshToken` zostało usunięte

### Test 5: Wygaśnięcie refresh tokena
1. Zaloguj się do aplikacji
2. Zmień czas systemowy o 8 dni do przodu (lub poczekaj 7 dni)
3. Przeładuj stronę
4. Spróbuj wykonać operację wymagającą autoryzacji
5. **Oczekiwany rezultat:**
   - Refresh token wygasł
   - Automatyczne odświeżenie nie powiedzie się
   - Użytkownik zostanie wylogowany z komunikatem "Sesja wygasła. Zaloguj się ponownie."

## Konfiguracja backendowa

Backend używa ASP.NET Core Identity API endpoints:
```csharp
// NetPcContacts.Infrastructure/Extensions/ServiceCollectionExtensions.cs:25-30
services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<NetPcContactsDbContext>()
    .AddApiEndpoints();

// Access token wygasa po 1 minucie (konfiguracja dla testowania)
services.ConfigureAll<BearerTokenOptions>(option =>
    option.BearerTokenExpiration = TimeSpan.FromMinutes(1));
```

Endpointy Identity API:
- `POST /api/identity/login` - logowanie
- `POST /api/identity/register` - rejestracja
- `POST /api/identity/refresh` - odświeżanie tokena
- Rate limiting: 10 requests/minute dla endpointów Identity

## Zgodność z Angular Best Practices

Wszystkie zmiany są zgodne z wytycznymi w `CLAUDE.md`:

✅ **Standalone Components** - brak użycia NgModules
✅ **Zoneless Change Detection** - używamy signali
✅ **Signals** - state management przez `signal()` i `computed()`
✅ **Signal Effects Pattern** - side effects w `untracked()`
✅ **Functional Interceptors** - `jwtInterceptor` i `errorInterceptor` jako funkcje

## Podsumowanie

Głównym problemem było **nieprawidłowe zapisywanie refresh tokena jako sesyjnego ciasteczka** bez atrybutów `expires`, `path` i bezpieczeństwa. Po naprawie:

- ✅ Refresh token jest zapisywany jako **persistent cookie** z 7-dniową ważnością
- ✅ Token przetrwa przeładowanie strony i zamknięcie przeglądarki
- ✅ Automatyczne odświeżanie tokena działa poprawnie
- ✅ Kod jest zgodny z Angular best practices (signals, untracked)
- ✅ Spójna architektura - wszystkie komponenty używają signali

Naprawa została przeprowadzona z zachowaniem zgodności z Clean Architecture i CQRS pattern używanym w projekcie.
