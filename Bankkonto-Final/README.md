## Funktioner: varför och hur

### 1) Budget/utgiftskategorier

- Varför
  - Ger snabb överblick över vart pengarna går (t.ex. Mat, Hyra, Transport).
  - Möjliggör filtrering och enklare rapportering i historiken utan extra datakällor.
  - Enum håller kategorier konsekventa, lätta att utöka och stabila i JSON.

- Hur
  - Datamodell: `ExpenseCategory` definieras i `Domain/Types.cs` (None, Food, Rent, Transport).
  - Transaktioner bär kategorin via `Transaction.Category` (se `Domain/Transaction.cs`), sätts vid uttag och överföring.
  - UI: Kategori väljs i `Pages/Payments.razor` via `<InputSelect>`; en enkel lista av tillgängliga kategorier visas för användaren.
  - Historik: Filtrering på kategori i `Components/History.cs` för att bryta ut kostnader per typ.

- Att utöka
  - Lägg till nya värden i `ExpenseCategory` och exponera dem i UI (listan i `Payments.razor`).

### 2) Export/Import (JSON) med validering

- Varför
  - Manuell backup/restore utan server; lätt att dela, inspektera och felsöka.
  - Mänskligt läsbart JSON underlättar testning och versionering.

- Hur
  - Export: `AccountService.ExportJsonAsync()` serialiserar konton med `System.Text.Json` och `JsonStringEnumConverter` (enum som strängar, indenterat JSON).
  - Import: `AccountService.ImportJsonAsync(json, replaceExisting)` validerar tom sträng, ogiltig JSON och tom lista; returnerar felmeddelanden. Vid merge dedupliceras på `Id`; vid replace ersätts allt.
  - UI: Sidan `Pages/Backup.razor` kopierar export till urklipp och importerar från en textarea.
  - Lagring: Underliggande persistens sker i webbläsarens `localStorage` via `Services/StorageService.cs` (JSON in/ut).

- Användning
  - Gå till “Backup”. Välj “Copy JSON” för export. Klistra in JSON och klicka “Import (merge)” för att importera data (utan att skriva över allt).

### 3) Åtkomstskydd: enkel PIN (UI‑lås)

- Varför
  - Snabb UI‑spärr för demosyfte/skolprojekt utan backend – hindrar nyfikna blickar men inte en angripare.
  - Minimalt friktion, enkelt att förstå och testa.

- Hur
  - Tjänst: `PinLockService` exponerar `IsUnlocked` och sparar låsstatus i `localStorage`.
  - Upplåsning: Hårdkodad PIN "9867" i `Services/PinLockService.cs` (byt vid behov).
  - Flöde: Startsidan `Pages/Pin.razor` ber om PIN och navigerar vidare vid godkänd PIN.
  - Guards: Sidor som `CreateAccount`, `Payments` och `History` skyddar sig och redirectar till PIN-sidan om låst. Menyn (`Layout/NavMenu.razor`) visar bara skyddade sidor när upplåst.
  - Registrering: Tjänster registreras för DI i `Program.cs`.

- Begränsningar
  - Ingen riktig autentisering eller kryptering; PIN är hårdkodad och status sparas i klartext i `localStorage`. Detta är medvetet, givet kravet “enkel UI‑låsning”.

- Tips
  - Byt PIN i `Services/PinLockService.cs`.
  - Uppgradera senare till riktig auth (t.ex. OIDC) om säkerhet behövs.

## Arkitektur i korthet

- Lager och gränssnitt
  - Domänmodeller: `BankAccount`, `Transaction`, enumtyper i `Domain/*`.
  - Tjänster: `IAccountService`, `ITransactionService`, `IStorageService`, `IPinLockService` med implementationer i `Services/*`.
  - UI: Razor‑sidor/komponenter i `Pages/*`, `Components/*`, `Layout/*` som bara pratar via tjänstegränssnitt.
  - DI: Registrerat i `Program.cs` för enkel testbarhet och utbytbarhet.

## Tekniska val och avvägningar

- Blazor WASM utan backend
  - + Enkel distribution, låg komplexitet, fungerar helt i webbläsaren.
  - – All logik och data finns i klienten (säkerhet begränsad). Passar demo/skolprojekt.

- `localStorage` för persistens
  - + Räcker för små datamängder, inga externa beroenden, enkelt API via `IJSRuntime`.
  - – Klartext, kvot (~5–10 MB), inga transaktioner/nyckelindex. Vid växande data: överväg IndexedDB eller server‑API.

- JSON‑serialisering med `System.Text.Json`
  - + Inbyggt i .NET, snabbt och lättviktigt. `JsonStringEnumConverter` + camelCase ger stabilt, läsbart JSON över språk.
  - `WriteIndented = true` prioriterar läsbarhet framför minimal storlek (rimligt för liten data och enkel debugging).

- Importmerge baserat på `Id` (GUID)
  - + Förhindrar dubbletter på ett entydigt sätt. Namn/type‑matchning undviks för att slippa kollisioner.
  - Alternativet “replace” förenklar total återställning utan specialfall.

- Enum för utgiftskategorier
  - + Kompilatorsäkerhet, konsekvent UI/JSON, lätt att utöka i kod.
  - – Mindre flexibelt än fritext. Om användardefinierade taggar behövs kan en separat tagg‑modell införas.

- UI‑lås (PIN) i stället för riktig auth
  - + Minimalt beroende, snabb att förstå/använda, tillräckligt för att gömma innehåll.
  - – Ingen verklig säkerhet. För skarp drift: använd riktig autentisering (t.ex. OIDC) och server‑lagring.

- Felhantering och loggning
  - Tjänster använder guard‑checks och returnerar fel (t.ex. lista vid import). Konsolutskrift räcker för felsökning i demo.

## Framtida förbättringar

- Riktig autentisering/auktorisering och server‑lagring av data.
- Byt `localStorage` mot IndexedDB för större datamängder och bättre robusthet.
- Användaregendefinierade kategorier/taggar med enkel sökbarhet.
- Export/import av både konton och transaktionshistorik samt checksummor för integritet.
