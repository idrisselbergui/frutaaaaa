# Error Handling Skill

## Purpose

Explain how this project currently logs, swallows, propagates, and returns errors across frontend, backend, sessions, and audit code.

## Key files and locations

- `fruta-client/src/apiService.js`
- `fruta-client/src/LoginForm.jsx`
- `fruta-client/src/pages/*.jsx`
- `frutaaaaa/Controllers/*.cs`
- `frutaaaaa/Audit/SessionController.cs`
- `frutaaaaa/Audit/AuditInterceptor.cs`
- `frutaaaaa/Audit/AuditActionFilter.cs`

## Backend conventions

- Many controller actions wrap the whole method body in `try/catch`.
- Common pattern:

```csharp
catch (Exception ex)
{
    return StatusCode(500, $"An error occurred: {ex.Message}");
}
```

- Some actions return `new { message = "..." }` instead.
- Some actions prefer `BadRequest("...")`, others `BadRequest(new { message = "..." })`.

## Frontend conventions

- `apiService.js` throws a JavaScript `Error`.
- It first tries `response.json()` and falls back to `HTTP error! status: ...`.
- Pages usually do one of:
  - `console.error(...)`
  - `setError(...)`
  - `setMessage(...)`
  - `alert(...)`

Real helper behavior:

```js
if (!response.ok) {
  const errorData = await response.json().catch(() => ({
    message: `HTTP error! status: ${response.status}`
  }));
  throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
}
```

## Silent-failure zones

These are intentional and should be preserved unless you are explicitly changing behavior:

- session failed-attempt tracking
- session start
- session page tracking
- session end
- sendBeacon session close
- audit interceptor write failures
- `ApplicationDbContext.OnConfiguring` audit-interceptor resolution failures

The principle in this codebase is: audit/session failures must not block business actions.

## Step by step: add error handling in a new controller action

1. Match the surrounding controller style first.
2. If the endpoint serves a page that expects `error.message`, prefer JSON `{ message = ... }`.
3. If the write is non-critical telemetry, consider fail-safe handling like `SessionController`.
4. Log enough to the console for local debugging because there is no centralized logging platform in repo.

## Step by step: debug an opaque frontend error

1. Inspect the browser console for `API request failed:`.
2. Check whether the backend returned JSON or plain text.
3. Check whether `X-Database-Name` was missing, because that often causes internal DB errors that surface as generic 500s.
4. For write endpoints, check whether business data saved even if audit/session logging failed.

## Dependencies

- `apiService.js`
- controller response shape
- browser console
- audit/session fail-safe philosophy

## Gotchas

- Some endpoints return `Ok(null)` instead of an error. `SampleController.GetDailyCheck` is the clearest example.
- Catch blocks sometimes leak internal exception messages directly to clients.
- There is no structured logging abstraction; many files use `Console.WriteLine`.

## Real example from this codebase

`AuditInterceptor` intentionally protects business writes:

```csharp
catch (Exception ex)
{
    Console.WriteLine($"[AuditInterceptor] Failed to write audit log: {ex.Message}");
}
```

If you add stricter error behavior here, you can break every non-GET mutation in the application.
