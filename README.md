# **BankApp**





Detta är mitt projekt i kursen OPG.
Appen är byggd i Blazor WebAssembly och fungerar som en enkel banksimulator.
Fokus har varit på att använda objektorienterad programmering, tjänster (services) och lokal datalagring via webbläsaren.



###### Funktioner:



**Inloggning -**

Appen startar alltid låst och kräver en 4-siffrig PIN-kod (7788) för att låsas upp.
PIN-låset hanteras av PinLockService, som sparar upplåst status i LocalStorage.

Konton

Det går att skapa och ta bort konton av typerna Baskonto, Sparkonto och Företagskonto.
Varje konto visar namn, kontotyp, valuta och aktuellt saldo.



**Transaktioner -**

Användaren kan sätta in, ta ut och överföra pengar mellan sina konton.
Belopp och validering kontrolleras, så man inte kan ange felaktiga värden.
Alla transaktioner sparas automatiskt.



**Historik -**

Alla konton har en historik över sina transaktioner.
Transaktionerna kan sorteras efter datum eller belopp.

Lokal datalagring

All data sparas lokalt i webbläsarens LocalStorage, så konton och historik finns kvar efter att appen stängts.

###### 

###### VG-delar:



**Export/Import (JSON) kontrollerad -**

Appen låter användaren exportera alla konton och transaktioner till en JSON-fil och senare importera dem igen.
Vid import kontrolleras att filen är giltig och att alla transaktioner hör till befintliga konton.
Användaren kan välja att ersätta all data eller lägga till den nya informationen via en enkel sida i gränssnittet.



**Automatisk ränta på sparkonton -**

Sparkonton får automatiskt 2.5% årlig ränta, som beräknas och läggs till var 60:e sekund.
Räntan räknas med metoden CalcInterest och appliceras av AutoApplyInterestToAllAsync.
Varje gång ränta läggs till registreras den som en ny transaktion med noten “Ränta”.



**PIN-låsning -**

Appen är skyddad med PIN vid uppstart och kan låsas manuellt via menyn.
Användaren måste ange rätt PIN för att kunna använda appen.
Det finns en "logga ut" knapp i menyn, man loggas inte ut automatiskt.





###### Tekniska val


Objektorienterad design

Appen är uppdelad i tydliga klasser som skiljer på data (t.ex. BankAccount, Transaction) och logik (AccountService).
Gränssnitt som IAccountService och IStorageService används för att hålla koden ren och utbyggbar.

Services och beroendeinjektion

All affärslogik ligger i AccountService, medan LocalStorageService hanterar lagringen.
Tjänsterna kopplas ihop via Dependency Injection så att koden är lös kopplad och lätt att ändra.

Lokal lagring

Appen använder Blazored.LocalStorage för att spara data direkt i webbläsaren.
Det behövs ingen server eller databas, allt sköts lokalt.



###### Kom igång:



Systemkrav

.NET 8.0 SDK

En modern webbläsare (Edge, Chrome, Firefox etc.)

Starta projektet

Gå till projektmappen:

cd BankApp



Kör:

dotnet run --project BankApp



Öppna den adress som visas i terminalen (t.ex. http://localhost:5034).

Ange PIN-koden 7788 för att logga in.

