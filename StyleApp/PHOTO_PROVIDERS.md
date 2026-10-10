# Photo providers

The application uses `IStylePhotoService` in its MVC layer and routes photo searches to the provider selected by `PhotoApi:Provider`. Providers implement `IStylePhotoProvider` and return the normalized `StyleLook` model, including the image URL, tags, season/style labels, photo page, and attribution.

The current provider is Unsplash:

```json
"PhotoApi": {
  "Provider": "Unsplash"
}
```

Set its access key outside source control, for example with the `Unsplash__AccessKey` environment variable or ASP.NET Core user secrets.

To add Pinterest or another source:

1. Implement `IStylePhotoProvider`, mapping the provider's API response and required attribution/links into `StyleLook`.
2. Register its typed HTTP client and expose it as `IStylePhotoProvider` in `Program.cs`, following the Unsplash registration.
3. Configure `PhotoApi:Provider` to match the new implementation's `ProviderName`.
4. Add provider-specific options and keep credentials in user secrets or environment variables.

The controller and views continue to use `IStylePhotoService`; provider-specific API details stay in the provider adapter. Selecting a provider that has not been registered displays a setup message instead of failing app startup.

# Photo provider

StyleApp reads outfit images from the SQL Server database populated by PinterestLinkSearcher. It uses `PinterestPins.Url` as the image URL and parses `PinterestPins.Tags` as a JSON string array.

Configure the provider and connection string in `appsettings.json` or through environment configuration:

```json
{
  "PhotoApi": {
    "Provider": "Pinterest"
  },
  "ConnectionStrings": {
    "PinterestPins": "Server=localhost;Database=PinterestPins;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Manually include a gender tag in each row's `Tags` JSON array, using `men` or `women` (also recognizes `man`, `woman`, `male`, and `female`). Gender is required for a result. Other requested tags are OR-matched against the stored tag phrases. For example:

```json
["women", "fuchsia pink shift dress", "nude pumps"]
```

The Pinterest provider implements `IStylePhotoProvider`; the MVC layer continues to use `IStylePhotoService`.
