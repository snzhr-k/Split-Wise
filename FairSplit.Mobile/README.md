# FairSplit.Mobile

React Native client built with Expo for the FairSplit backend.

## Recommended repository structure

At the workspace root:

- `FairSplit.Backend/` ASP.NET Core API + PostgreSQL
- `FairSplit.Mobile/` Expo React Native client
- shared docs (`openapi.yaml`, planning markdown files)

This keeps backend and mobile independent while still in one repository.

## Mobile folder structure

```text
FairSplit.Mobile/
  App.tsx
  app.json
  .env.example
  src/
    api/
      httpClient.ts
    config/
      env.ts
    screens/
      RootScreen.tsx
    components/
    features/
      README.md
    types/
    utils/
```

## Important files

- `App.tsx`: thin entry point that renders the root screen.
- `src/screens/RootScreen.tsx`: placeholder UI for app bootstrap state.
- `src/config/env.ts`: API base URL resolution and platform-aware fallback values.
- `src/api/httpClient.ts`: tiny reusable API client wrapper.
- `.env.example`: sample API URL values for simulator/emulator/device.
- `src/features/README.md`: conventions for feature-based growth.

## Local development workflow

1. Start PostgreSQL locally.
2. Start backend API:
   - `cd FairSplit.Backend/src/FairSplit.Api`
   - `ASPNETCORE_URLS=http://localhost:5000 dotnet run`
    - For physical device testing on the same Wi-Fi, use `ASPNETCORE_URLS=http://0.0.0.0:5000 dotnet run`.
3. Start mobile app in another terminal:
   - `cd FairSplit.Mobile`
   - `npm install`
   - `npm start`
4. Open the Expo app:
   - iOS simulator (`i` in Expo terminal)
   - Android emulator (`a` in Expo terminal)
   - Expo Go on a physical device (QR code)

## API base URL configuration

Expo reads variables prefixed with `EXPO_PUBLIC_`.

1. Copy `.env.example` to `.env`.
2. Set `EXPO_PUBLIC_API_BASE_URL` based on target:
   - iOS simulator: `http://localhost:5000`
   - Android emulator: `http://10.0.2.2:5000`
   - Physical device: `http://<YOUR_MAC_LAN_IP>:5000`
3. Restart Expo after changing `.env`.

If backend runs on a different port, update the URL accordingly.

## Authentication token configuration

If you want the mobile client to call protected backend endpoints, set:

- `EXPO_PUBLIC_DEV_AUTH_TOKEN` in `.env`

Current status (May 2026):

- The app supports bearer auth headers through `src/api/httpClient.ts`.
- The current implementation uses a dev token from `.env`.
- A full user login screen/token persistence flow is not implemented yet.

You can obtain a token from the backend dev endpoint:

```bash
curl -sS -X POST http://localhost:5000/api/auth/dev-token \
  -H 'Content-Type: application/json' \
  --data-binary '{"memberId":"22222222-2222-2222-2222-222222222222","displayName":"Alice"}'
```

Then copy the returned `accessToken` into `EXPO_PUBLIC_DEV_AUTH_TOKEN` and restart Expo.

## iOS Simulator / API Troubleshooting

If groups load but Create Expense shows member-loading errors, verify backend endpoint coverage and port mapping:

1. Backend is running on a reachable host/port (example used during verification):

```bash
ASPNETCORE_URLS=http://0.0.0.0:5001 dotnet run
```

2. Mobile `.env` points to the same backend:

```env
EXPO_PUBLIC_API_BASE_URL=http://<YOUR_MAC_LAN_IP>:5001
EXPO_PUBLIC_DEV_AUTH_TOKEN=<dev-token>
```

3. Restart Expo after any `.env` change.

4. Quick backend checks:

```bash
curl -sS -i http://<YOUR_MAC_LAN_IP>:5001/api/groups
curl -sS -i http://<YOUR_MAC_LAN_IP>:5001/api/groups/11111111-1111-1111-1111-111111111111/members
```

For Expo Web testing, browser CORS rules apply; make sure your ASP.NET Core CORS policy allows requests from the Expo Web origin.

## Notes

- Current Node in your environment may be older than Expo's recommended version.
- Use Node 20 LTS (or newer) for smoother local development.
