# HotelFlow Frontend

React + Vite + TypeScript + Tailwind CSS frontend for the HotelFlow hotel management system.

## Prerequisites

- Node.js 18+
- The backend running at `https://localhost:7202`

## Setup & Run

```bash
# Install dependencies
npm install

# Start dev server (runs on http://localhost:3000)
npm run dev
```

## Project Structure

```
src/
├── api/              # All API calls (one file per domain)
│   ├── client.ts     # Axios instance + JWT auto-refresh interceptor
│   ├── auth.ts
│   ├── rooms.ts
│   ├── reservations.ts
│   ├── housekeeping.ts
│   └── users.ts
├── components/
│   ├── Layout.tsx    # Sidebar + navigation (role-aware)
│   ├── ProtectedRoute.tsx
│   └── ui.tsx        # Shared UI: badges, modal, spinner, stat card…
├── context/
│   └── AuthContext.tsx  # JWT auth + role detection
├── pages/
│   ├── auth/         # Login, Register
│   ├── guest/        # Dashboard, Browse Rooms, My Reservations
│   ├── staff/        # Dashboard, All Reservations, Checkouts, Rooms, Users
│   └── housekeeping/ # Dashboard, My Tasks, Available Tasks, All Tasks
├── types/
│   └── index.ts      # TypeScript types matching backend DTOs
├── App.tsx           # Router with role-based route guards
└── main.tsx
```

## Roles & Dashboards

| Role | Route | Access |
|---|---|---|
| `Guest` | `/guest` | Browse rooms, make reservations, view own bookings |
| `Staff` | `/staff` | Full reservation management, check-in/out, rooms CRUD, user management |
| `Housekeeping` | `/housekeeping` | View today's tasks, take available tasks, complete tasks |

## Auth

- JWT Bearer tokens (access + refresh)
- Access token stored in `localStorage` as `accessToken`
- Refresh token stored as `refreshToken`
- Axios interceptor auto-refreshes on 401 responses
- On token expiry, redirects to `/login`

## Backend

Base URL: `https://localhost:7202`

Make sure CORS is enabled in the backend `Program.cs` for `http://localhost:3000`.
If CORS isn't configured yet, add this before `app.Build()`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

And in the middleware pipeline:

```csharp
app.UseCors();
```
