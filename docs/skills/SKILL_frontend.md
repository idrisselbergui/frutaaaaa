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

## Standardized Pagination Rule

To ensure a consistent, premium UI/UX across the entire application, all paginated pages must adhere to the standardized pagination design system.

### 1. Pagination Logic (useMemo Centered Ellipsis Truncation)
Avoid displaying all page buttons at once. Instead, compute page numbers dynamically using `useMemo` with centered ellipsis truncation:

```jsx
const pageNumbers = useMemo(() => {
    const pages = [];
    const maxVisible = 5;

    if (totalPages <= maxVisible) {
        for (let i = 1; i <= totalPages; i++) {
            pages.push(i);
        }
    } else {
        // Always show the first page
        pages.push(1);

        // Calculate start and end for middle block centered on currentPage
        let start = Math.max(2, currentPage - 1);
        let end = Math.min(totalPages - 1, currentPage + 1);

        // Adjust if we are close to boundaries
        if (currentPage <= 3) {
            end = 4;
        } else if (currentPage >= totalPages - 2) {
            start = totalPages - 3;
        }

        // Add left ellipsis before middle block if needed
        if (start > 2) {
            pages.push('...');
        }

        // Add middle block page numbers
        for (let i = start; i <= end; i++) {
            pages.push(i);
        }

        // Add right ellipsis after middle block if needed
        if (end < totalPages - 1) {
            pages.push('...');
        }

        // Always show the last page
        pages.push(totalPages);
    }
    return pages;
}, [currentPage, totalPages]);
```

### 2. Standardized HTML Markup
Use the exact same HTML elements and class names to structure the pagination block to avoid clipping and style inconsistencies:

```jsx
{totalPages > 1 && (
    <div className="pagination-container">
        {totalItems > 0 && (
            <div className="pagination-info">
                Page {currentPage} sur {totalPages} ({totalItems} éléments)
            </div>
        )}
        <div className="pagination">
            <button
                className="pagination-nav"
                onClick={() => setCurrentPage(c => Math.max(1, c - 1))}
                disabled={currentPage === 1}
                aria-label="Previous page"
            >
                <span className="nav-arrow">«</span> Précédent
            </button>
            <div className="pagination-numbers">
                {pageNumbers.map((p, i) => (
                    p === '...' ? (
                        <span key={`ellipsis-${i}`} className="pagination-ellipsis">...</span>
                    ) : (
                        <button
                            key={p}
                            className={`pagination-number ${currentPage === p ? 'active' : ''}`}
                            onClick={() => setCurrentPage(p)}
                        >
                            {p}
                        </button>
                    )
                ))}
            </div>
            <button
                className="pagination-nav"
                onClick={() => setCurrentPage(c => Math.min(totalPages, c + 1))}
                disabled={currentPage === totalPages}
                aria-label="Next page"
            >
                Suivant <span className="nav-arrow">»</span>
            </button>
        </div>
    </div>
)}
```

### 3. Unified CSS Styles
All navigation buttons (`.pagination-nav`) and number buttons (`.pagination-number`) must be `48px` high with `8px` border-radius and standard hover/active blue themes. Navigation buttons must use `width: auto` to prevent text truncation:

```css
/* Modern Pagination Styles */
.pagination-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.5rem;
  margin-top: 2.5rem;
  padding: 1.5rem 0;
  font-family: system-ui, -apple-system, sans-serif;
}

.pagination-info {
  font-size: 1rem;
  color: #6b7280;
  font-weight: 600;
  text-align: center;
}

.pagination {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
  justify-content: center;
}

.pagination-nav {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 48px;
  padding: 0 1.5rem !important; /* Force padding to prevent text truncation */
  width: auto !important; /* Override any global 48px constraints */
  min-width: fit-content !important;
  max-width: none !important;
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  color: #007bff;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  user-select: none;
  box-sizing: border-box; /* Force padding inside dimensions */
}

.pagination-nav:hover:not(:disabled) {
  background: #f8fafc;
  border-color: #007bff;
  color: #0056b3;
}

.pagination-nav:disabled {
  opacity: 0.5;
  background: #f9fafb;
  color: #9ca3af;
  cursor: not-allowed;
  border-color: #e5e7eb;
}

.pagination-ellipsis {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  color: #6b7280;
  font-size: 1.1rem;
  font-weight: bold;
  user-select: none;
}

.nav-arrow {
  font-size: 1.2rem;
  font-weight: bold;
  line-height: 1;
  margin: 0 4px;
  display: inline-block;
  transform: translateY(-1px);
}

.pagination-numbers {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0 0.5rem;
}

.pagination-number,
.pagination-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  background: white;
  color: #374151;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.pagination-number:hover,
.pagination-btn:hover:not(:disabled) {
  border-color: #007bff;
  color: #007bff;
}

.pagination-number.active,
.pagination-btn.active {
  background: #007bff;
  border-color: #007bff;
  color: white;
  font-weight: 600;
  box-shadow: 0 4px 6px rgba(0, 123, 255, 0.2);
}
```

