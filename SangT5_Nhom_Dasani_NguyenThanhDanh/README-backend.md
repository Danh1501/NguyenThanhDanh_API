# AttendanceApi backend

ASP.NET Core Web API for the Android attendance app. Structure follows the course pattern: Controllers → Services → Repositories → EF Core / SQL Server.

## Folders

- `Controllers`: `AuthController`, `EmployeesController`, `ShiftsController`, `AttendanceController`, and `ReportsController`.
- `Data`: `ApplicationDbContext` and development database seeding.
- `DTOs`: flat request models (`LoginRequest`, `RegisterRequest`, `VerifyOtpRequest`, `AttendanceRequest`) plus CRUD request models.
- `Models`: `ApplicationUser`, `Employee`, `Shift`, `AttendanceRecord`, and `OtpCode`.
- `Repositories`: flat repository interfaces and implementations for employees, shifts, and attendance.
- `Services`: authentication, JWT, OTP, and attendance business logic.

Repository and service files are kept directly inside their folders so the project follows the agreed flat structure. Shift and employee CRUD DTOs remain alongside the four primary request DTOs because those endpoints use them.

## Local development

1. Copy `appsettings.example.json` to `appsettings.json`, then set `ConnectionStrings:DefaultConnection` to your SQL Server instance. `appsettings.json` is ignored by Git because it contains machine-specific settings.
2. Start the API with the `http` launch profile in Visual Studio. Development uses HTTP for the Android Emulator; use `http://10.0.2.2:5193` as the API base address from the emulator.
3. The startup creates tables and the two roles. It does not create demo accounts or sample attendance records.

Face recognition is used only during attendance; normal sign-in remains password + email OTP or GitHub. Users register their face from the Flutter app after signing in. The backend stores only the face embedding, not the uploaded photos. Face matching currently has no liveness/anti-spoof check.

The face image pipeline directly references SixLabors.ImageSharp 4.1.2. Obtain a license from [Six Labors](https://licensing.sixlabors.com/) for this project assembly (`SangT5_Nhom_Dasani_NguyenThanhDanh`), then set the full issued key in the build environment before building. In PowerShell:

```powershell
$env:SIXLABORS_LICENSE_KEY = '<full-issued-license-key>'
dotnet build
```

Do not commit the key. The API compiles without it but MSBuild emits a license warning; the warning can only be cleared with a valid key.

Email OTP is required; codes are random, expire after the configured time, and are never returned in the API response. From the backend project folder, run `dotnet user-secrets init`, then set `Smtp:Host=smtp.gmail.com`, `Smtp:Port=587`, `Smtp:EnableSsl=true`, `Smtp:Username=<sender email>`, `Smtp:Password=<Google app password>`, `Smtp:From=<sender email>`, and `Jwt:Secret=<long random secret>` with `dotnet user-secrets set`. For Gmail use an App Password rather than the account password. Keep SMTP credentials and signing secrets out of source control. Registration lets you choose Admin or Employee for the course project; on a public system, Admin creation should be restricted to an existing administrator.

GitHub sign-in is configured separately. Set `GitHub:ClientId`, `GitHub:ClientSecret`, and `GitHub:RedirectUri=sangt5attendance://oauth/github` in User Secrets. Create a GitHub OAuth App with that exact callback URI and pass its Client ID to Flutter as `--dart-define=GITHUB_CLIENT_ID=...`. The app uses authorization code + PKCE; the client secret stays on the backend. See `GITHUB_SIGN_IN_SETUP.md` in the Flutter project for step-by-step setup.

## Main endpoints

- `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/verify-otp`
- `POST /api/auth/github` (OAuth authorization code + PKCE exchange)
- `POST /api/face/enroll`, `POST /api/face/verify`, `DELETE /api/face/profile` (authenticated face enrollment and attendance verification)
- Admin: CRUD ` /api/employees` and `/api/shifts`
- Employee: `POST /api/attendance/check-in`, `POST /api/attendance/check-out`, `GET /api/attendance/my`
- Admin: read/update/delete `/api/attendance`
- Admin: `GET /api/reports/attendance?from=YYYY-MM-DD&to=YYYY-MM-DD`
- `GET /api/health`, Swagger UI `/swagger`, OpenAPI JSON `/openapi/v1.json`

All protected endpoints use `Authorization: Bearer <token>`. Admin-only operations require the `Admin` role; employee attendance actions require `Employee`.

