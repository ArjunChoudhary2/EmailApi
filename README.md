# Email Sender Backend

.NET 9 Web API backend for Google OAuth sign-in, Gmail sending, JWT sessions, and PostgreSQL email history.

## Configuration

Set these environment variables before running:

```powershell
$env:GOOGLE_CLIENT_ID="your-google-client-id"
$env:GOOGLE_CLIENT_SECRET="your-google-client-secret"
$env:GOOGLE_REDIRECT_URI="http://localhost:5173/auth/google/callback"
$env:JWT_SECRET="replace-with-at-least-32-random-characters"
$env:JWT_ISSUER="EmailSender.Api"
$env:JWT_AUDIENCE="EmailSender.Frontend"
$env:FRONTEND_ORIGIN="http://localhost:5173,http://127.0.0.1:5173"
$env:DATABASE_URL="Host=localhost;Port=5432;Database=email_sender;Username=postgres;Password=postgres"
```

The Google OAuth client must allow the same redirect URI and request the Gmail send scope. For Supabase, use the Postgres connection string from Project Settings > Database, convert it to Npgsql format if needed, and set it as `DATABASE_URL`.

Example Supabase direct connection:

```powershell
$env:DATABASE_URL="Host=db.your-project-ref.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
```

If your host needs pooled connections, use Supabase's pooler host/port instead.

## Run

```powershell
dotnet run --project .\EmailSender.Api\EmailSender.Api.csproj
```

Swagger is available at `/swagger`.

For the React frontend, set `VITE_API_BASE_URL` if your backend is not running on `http://localhost:5222`. If Vite starts on a different port, add that exact browser origin to `FRONTEND_ORIGIN`.

## API

- `GET /api/auth/google/login`: returns Google consent URL and state.
- `POST /api/auth/google/callback`: accepts `{ "code": "...", "state": "..." }`, stores/updates the user and encrypted refresh token, then returns a JWT.
- `POST /api/emails/send`: sends an email from the authenticated user's Gmail account.
- `GET /api/emails/history`: returns the authenticated user's email attempts.
- `GET /api/profile`: returns the authenticated user's profile.
