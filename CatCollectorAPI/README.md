# CatCollector API — local setup notes

## JWT signing key

This project uses HS256 JWT tokens. HS256 requires a signing key of at least **256 bits (32 bytes)**.

Recommendation:
- For local development you can use the dev key defined in `appsettings.Development.json` (already present). This is convenient but **not** secure for production.
- For production, set the `Jwt__Key` environment variable (double underscore maps to `Jwt:Key` in .NET config). Example (Linux/macOS):

  export Jwt__Key="$(openssl rand -base64 32)"

  Or on Windows PowerShell:

  $Env:Jwt__Key = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 -Minimum 0 } | % { [byte]$_ }))

After setting the env var, restart the API:

  cd CatCollectorAPI
  dotnet run

If the key is too short the server will log a warning and token creation will fail with a clear message in the API response.

## .env usage (optional)
If you prefer, create a `.env` file (not committed) and use a tool to load it into your shell, or set `Jwt__Key` directly in your deployment environment.

