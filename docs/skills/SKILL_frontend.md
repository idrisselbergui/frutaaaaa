# Frontend Skill

## Purpose

Explain how the React SPA is organized, how pages fetch data, how route protection works, and how styling/layout are handled.

## Key files and locations

- `fruta-client/src/App.jsx`
- `fruta-client/src/main.jsx`
- `fruta-client/src/apiService.js`
- `fruta-client/src/ProtectedRoute.jsx`
- `fruta-client/src/components/Layout.jsx`
- `fruta-client/src/components/Sidebar.jsx`
- `fruta-client/src/pages/*.jsx`
- `fruta-client/src/utils/*.js`
- `fruta-client/src/hooks/useDebounce.js`

## Organization

- `App.jsx`
  - route table, login state, session tracking hooks
- `pages/`
  - one file per feature screen, often with most business UI logic inline
- `components/`
  - reusable layout, chart, modal, and widget pieces
- `utils/`
  - PDF export and formatting helpers
- `apiService.js`
  - single fetch abstraction used across the SPA

## Route pattern

Routes are declared centrally in `App.jsx`. Protected screens are wrapped like this:

```jsx
<Route path="/gestion-avance" element={
  <PageProtectedRoute user={user} pageName="gestion-avance">
    <GestionAvancePage />
  </PageProtectedRoute>
} />
```

If you add a new page, update:

1. `App.jsx`
2. navigation in sidebar/header as needed
3. `AdminPage.jsx` available permissions list
4. `UsersController.Register` default page list if new users need an explicit false row

## State and data-fetching conventions

- Pages typically use `useState` + `useEffect`.
- Shared state libraries are not used.
- `DashboardPage.jsx` is the heaviest page and uses:
  - `useDebounce`
  - multiple parallel API fetches
  - local `useMemo` sorting and option filtering
  - PDF export helpers from `utils`
- Most pages call `apiGet`, `apiPost`, `apiPut`, `apiDelete` directly.

Real example:

```jsx
const [destData, partData, grpData, palData] = await Promise.all([
  apiGet('/api/lookup/destinations'),
  apiGet(`/api/lookup/partenaires/${partnerType}`),
  apiGet('/api/lookup/grpvars'),
  apiGet('/api/lookup/tpalettes')
]);
```

## Styling rules actually used here

- Mostly plain CSS files colocated with components/pages.
- Some pages use a lot of inline styles, especially `AdminPage.jsx`.
- `react-select` controls are used for searchable dropdowns.
- `Layout.jsx` manages desktop/mobile sidebar behavior and persists collapse state in `localStorage`.

## Step by step: add or modify a page

1. Identify whether the page is route-level or a reusable component.
2. Fetch lookup/reference data through `apiService.js`, not raw `fetch`, unless you intentionally need the session helper flow.
3. Keep tenant behavior intact by relying on the existing API wrapper.
4. Preserve permission gating if the page should not be globally accessible.
5. If the page exports PDFs or charts, follow the `DashboardPage.jsx` and `utils/pdfGenerator.js` pattern rather than introducing a second export stack.

## Dependencies

- `react-router-dom`
- `apiService.js`
- `sessionStorage`
- backend response shapes
- chart/export packages:
  - `recharts`
  - `jspdf`
  - `jspdf-autotable`
  - `html2canvas`

## Gotchas

- The existing app already mixes CSS modules, plain CSS, and large inline-style objects. Stay consistent with the local file you are editing instead of forcing a repo-wide redesign.
- `apiService.js` only adds `Content-Type` when a body exists or method is not GET.
- Some UI text and comments contain mojibake/encoding issues. Preserve intent carefully when editing.
- `DashboardPage.jsx` is large and high-risk; small response-shape changes can break charts, tables, and PDF generation at once.

## Real example from this codebase

`Layout.jsx` is the source of truth for sidebar persistence:

```jsx
const [sidebarCollapsed, setSidebarCollapsed] = useState(() => {
  const stored = localStorage.getItem('sidebarCollapsed');
  return stored ? JSON.parse(stored) : false;
});
```

If you change sidebar behavior, update both the state logic and the `localStorage` writes in the same file.
