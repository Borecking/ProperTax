Link do dokumentacji w google docs z ładnym formatowaniem:

1. Opis działania projektu
2. Aplikacja służy do inwentarzu nieruchomości oraz obliczania podatku za ich powierzchnie. Dzięki temu można przeglądać aktualnie posiadane nieruchomości oraz te które zostały już sprzedane.

3. Technologia
Aplikacja internetowa została napisana w frameworku asp.net 8.0
Architektura: Model-View-Controller
Baza danych: Microsoft sql server
Użyto Entity framework do mapowania obiektowo-relacyjnego.

Korzysta z rozwiązań: 
Microsoft.AspNetCore.Identity.EntityFrameworkCore Microsoft.AspNetCore.Identity.UI
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools

3. Pierwsze uruchomienie projektu
Otworzyć przy pomocy Visual Studio.
W konsoli nuget wpisać update-database
Skompilować i uruchomić program.

4. Opis struktury projektu
Aplikacja jest w architekturze MVC. W folderze Controllers są 4 kontrolery: HomeController, NieruchomosciController, StawkiPodatkowController, SumyPowierzchniController. W models 4 modele: ErrorViewModel, Nieruchomosc, StawkaPodatku, SumaPowierzchni. W views Home, Nieruchomosci, Shared, StawkiPodatkow, SumyPowierzchni, _ViewImports, _ViewStart. W Data są migracje oraz ApplicationDbContext. W pliku appsettings.json jest łańcuch połączenia do bazy danych. Oraz Program.cs, który tworzy aplikacje, role użytkowników, użytkownika admina oraz ustawia DateTime na format yyyy-mm-dd.

Aby móc cokolwiek zrobić trzeba dodać nowy rok w “Stawki podatków”. Tam dodajemy rok oraz stawki podatków na różne kategorie powierzchni. Podczas dodawania w “Sumy powierzchni” również doda się 12 miesięcy tego roku. Tam obliczane są sumy powierzchni wszystkich nieruchomości posiadanych w danym miesiącu. Następnie można zacząć dodawać nieruchomości w “Nieruchomości”. Można dodać tylko wtedy kiedy rok daty zakupu został dodany w “Stawki podatków”. Aby dodać wymagany jest adres, numer księgi i data zakupu, ponieważ nie zawsze od razu wiemy wszystkie dane, które chcemy dodać - resztę można uzupełnić później. Jeśli dany rok w “Stawki podatków” zostanie usunięty to odpowiednie miesiące w “Sumy powierzchni” również oraz wszystkie nieruchomości, które były posiadane w danym roku. Dlatego usuwanie w “Stawki podatków” zarezerwowane jest dla admina.
