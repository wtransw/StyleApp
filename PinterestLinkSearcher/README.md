# PinterestLinkSearcher

Console utility that scrolls a Pinterest board in a visible Chromium window, collects unique pin URLs, and saves them to SQL Server. Pin tags are JSON stored in an `nvarchar(max)` column and are inferred from pin text/image alt text when Pinterest exposes them; this is not visual image recognition.

## Setup

1. Set `ConnectionStrings:DefaultConnection` in `PinterestLinkSearcher/appsettings.json`. Example for Windows Authentication:

	  ```json
	  "ConnectionStrings": {
	 "DefaultConnection": "Server=localhost;Database=PinterestPins;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

	  The app also accepts `ConnectionStrings__DefaultConnection` as an environment-variable override. Treat `appsettings.json` as sensitive if the connection string contains a SQL username/password, and do not commit those credentials.

2. Build the project and install Playwright Chromium once:

   ```powershell
   dotnet build .\PinterestLinkSearcher\PinterestLinkSearcher.csproj
   pwsh .\PinterestLinkSearcher\bin\Debug\net10.0\playwright.ps1 install chromium
   ```

3. Run the app, choose the collection menu option, and enter a board URL such as `https://www.pinterest.com/lookastic/celeb-style/`. The board column is set to the final URL path segment (`celeb-style`).

4. If Pinterest requires authentication, sign in manually in the opened browser. The browser profile is stored under the current user's local application data so the session can be reused on later runs.

On the first successful collection, the app creates the database and `PinterestPins` table if they do not already exist. The URL hash has a unique index; reruns skip duplicate URLs. The `datetime` value is recorded in UTC when a new pin is first saved.

Pinterest page markup and access rules can change. Respect Pinterest's terms, use conservative request rates, and stop if access is denied. The app does not automate account passwords.
