# SMS Bridge

SMS Bridge to mój projekt służący do przesyłania wiadomości SMS pomiędzy aplikacją desktopową a telefonem z Androidem.

Projekt składa się z kilku współpracujących komponentów:

- `SmsBridge.Desktop` — aplikacja Blazor z interfejsem do obsługi wiadomości,
- `SmsBridge.Relay` — usługa pośrednicząca w komunikacji,
- `SmsBridge.Shared` — współdzielone kontrakty komunikacyjne,
- klient mobilny Android,
- `SmsBridge.Simulator` — narzędzie pomocnicze do testowania przepływu wiadomości.

## Jak to działa

Desktop Client
    |
    v
Relay Server
    |
    v
Android Client
    |
    v
SMS

Komunikacja z relayem wykorzystuje SignalR.

## Aktualne funkcjonalności

- wyświetlanie wiadomości SMS w aplikacji desktopowej,
- przechowywanie wiadomości w pamięci aplikacji,
- lokalny endpoint `POST /api/messages`,
- walidacja danych wiadomości,
- aktualizacja UI po dodaniu nowych wiadomości,
- SignalR Hub w usłudze relay,
- współdzielony kontrakt `RelayMessageEnvelope`,
- połączenie aplikacji desktopowej z relayem,
- połączenie klienta mobilnego z relayem,
- przesyłanie strukturalnych payloadów SMS,
- symulator wiadomości,
- przekazywanie przychodzących SMS-ów z Androida.

## Technologie

- C#
- .NET
- Blazor
- ASP.NET Core
- SignalR
- Git

## Status

Projekt jest aktywnie rozwijany.
