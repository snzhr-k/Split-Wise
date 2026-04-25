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

For Expo Web testing, browser CORS rules apply; make sure your ASP.NET Core CORS policy allows requests from the Expo Web origin.

## Notes

- Current Node in your environment may be older than Expo's recommended version.
- Use Node 20 LTS (or newer) for smoother local development.
