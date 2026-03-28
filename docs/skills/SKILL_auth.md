# Auth Skill

## Purpose

Explain how login, page permissions, session tracking, and audit identity work in this codebase. Use this file before changing anything related to `users`, `permissions`, `sessionStorage`, custom headers, or `/api/sessions/*`.

## Key files and locations

- `fruta-client/src/LoginForm.jsx`
- `fruta-client/src/ProtectedRoute.jsx`
- `fruta-client/src/App.jsx`
- `fruta-client/src/apiService.js`
- `frutaaaaa/Controllers/UsersController.cs`
- `frutaaaaa/Audit/SessionController.cs`
- `frutaaaaa/Audit/AuditActionFilter.cs`
- `frutaaaaa/Audit/AuditInterceptor.cs`
- `frutaaaaa/Models/UserRequest.cs`
- `frutaaaaa/Models/UserPagePermission.cs`

## What actually authenticates a user

- The login form sends `database`, `username`, `password`, and `permission: 0` to `POST /api/users/login`.
- `UsersController.Login` opens the selected database and runs:

```csharp
var user = await dynamicContext.Users.FirstOrDefaultAsync(u =>
    u.Username == request.Username &&
    u.Password == request.Password);
```

- Success response contains:
  - `message`
  - `userId`
  - `database`
  - `permissions` shaped as `{ page_name, allowed }`
- There is no JWT, no cookie, no ASP.NET Identity, and no server session token tied to business requests.

## How authorization works

- `ProtectedRoute.jsx` checks only the `user.permissions` array stored in `sessionStorage`.
- If a page is not allowed, the user is redirected to `/home`.
- Backend controllers are not protected with `[Authorize]`, roles, or policies.

Template copied from the real route guard:

```jsx
const hasPagePermission = (user, pageName) => {
  if (user?.permissions && Array.isArray(user.permissions)) {
    const permission = user.permissions.find(p => p.page_name === pageName);
    return permission && permission.allowed === 1;
  }
  return false;
};
```

## Session tracking flow

### On successful login

1. `LoginForm.jsx` stores a machine fingerprint in `sessionStorage.machineName`.
2. It derives `browser` and `os` from `navigator.userAgent`.
3. It calls `postSessionStart({ userId, username, tenantDatabase, machineName, browser, os })`.
4. If successful, it stores `sessionId` in `sessionStorage`.
5. Then it stores the user object and navigates to `/home`.

### On route change

- `App.jsx` posts to `/api/sessions/page` whenever `location.pathname` changes and `sessionId` exists.

### On logout

- `App.jsx` posts `/api/sessions/end` with status `LOGGED_OUT`, then clears:
  - `user`
  - `machineName`
  - `sessionId`

### On tab close

- `sendBeaconSessionEnd` uses `navigator.sendBeacon` with status `TAB_CLOSED`.

## Audit identity headers

Every normal API request built by `apiService.js` includes:

```js
headers.append('X-User-Id', user?.userId != null ? String(user.userId) : '');
headers.append('X-Username', user?.username || '');
headers.append('X-Machine-Name', sessionStorage.getItem('machineName') || '');
```

`AuditActionFilter` reads those headers and stores them in a scoped `AuditContext`. `AuditInterceptor` later uses that context when EF writes audit rows.

## Step by step: add a new permission-protected page

1. Add the page component under `fruta-client/src/pages`.
2. Register a route in `fruta-client/src/App.jsx` using `PageProtectedRoute`.
3. Choose a stable `pageName` string.
4. Add that same page id to `availablePages` in `fruta-client/src/pages/AdminPage.jsx`.
5. If you need new users to receive a default false permission row, add the page id to `availablePages` inside `UsersController.Register`.
6. Verify admin permission editing still posts the exact `PageName` value.

If step 4 or 5 is skipped, admins or newly created users will not see correct permission state.

## Step by step: debug a login issue

1. Check what database name the user entered in `LoginForm.jsx`.
2. Confirm `UsersController.Login` can open that tenant DB by string-replacing `frutaaaaa_db`.
3. Confirm the `users` table in that tenant DB contains plaintext values matching the request.
4. Inspect the browser `sessionStorage` keys after login:
   - `user`
   - `machineName`
   - `sessionId`
5. If login succeeds but routing fails, inspect the `permissions` array shape returned by the backend.

## Dependencies

- Tenant DB selection via `X-Database-Name`
- `sessionStorage` in the browser
- `AuditActionFilter` and `AuditInterceptor`
- `AuditDbContext` journal connection
- `ip-api.com` inside `SessionController.StartSession`

## Gotchas

- Backend “authorization” is not security. Anyone who can call the API can bypass the React guard.
- Passwords are not hashed.
- `UserRequest.Permission` still exists in `UserRequest.cs` but is largely obsolete in the current page-permission system.
- Session tracking endpoints intentionally swallow many failures; missing session records may not block the UI.
- Audit identity is header-based, so forged requests can spoof `X-Username` and `X-User-Id`.

## Real example from this codebase

The real login response shape in `UsersController.Login` is:

```csharp
return Ok(new
{
    message = "Login successful",
    userId = user.Id,
    database = request.Database,
    permissions = permissions.Select(p => new {
        page_name = p.PageName,
        allowed = p.Allowed ? 1 : 0
    }).ToList()
});
```

Build new auth-adjacent code around this exact casing unless you are changing both frontend and backend together.
