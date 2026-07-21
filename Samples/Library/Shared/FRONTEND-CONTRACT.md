# Frontend contract

The library sample ships **two** frontends over the same API:

| Frontend | Project | Rendering |
|---|---|---|
| Razor | `Core/Pages` | Server-rendered, full page load per navigation, `<form>` POST |
| React | `Web` | Client-rendered SPA, `fetch` against the API, no page reloads |

They behave differently on purpose — that is the point, since the two produce visibly different HTTP and
telemetry shapes for the Prologue Extractor to capture. **They must look identical**, and **one Playwright suite
drives both**.

Two rules make that work.

## 1. One stylesheet

`Shared/library.css` is the only stylesheet. Neither frontend keeps a copy:

- **Core** links it into `wwwroot/css/library.css` via an MSBuild `Content` item.
- **Web** imports it directly (`import '../Shared/library.css'`), with Vite's `server.fs.allow` widened to reach it.

Never add a second stylesheet, and never inline styles that change layout or color. If something needs a new
style, add a class to `library.css` and use it from both.

## 2. Identical markup skeleton and `data-testid`s

Both frontends render the same element structure with the same class names, so the shared stylesheet lands the
same way. Every element a test touches carries the same `data-testid` in both.

### Page skeleton

```html
<div class="app">
  <header class="app-header">
    <div class="brand">Library</div>
    <nav class="nav">
      <a class="nav-link active" data-testid="nav-authors" href="...">Authors</a>
      <!-- members, catalog, inventory, reservations, loans, simulation -->
    </nav>
    <div class="badge" data-testid="frontend-kind">Razor</div>   <!-- or "React" -->
  </header>
  <main class="page">
    <h1 class="page-title" data-testid="page-title">Authors</h1>
    <p class="page-subtitle">…</p>

    <div class="message message-error" data-testid="error">…</div>   <!-- only when there is one -->

    <section class="card">
      <h2 class="card-title">Register author</h2>
      <form class="form-row" data-testid="author-form"> … </form>
    </section>

    <section class="card">
      <div class="table-scroll">
        <table class="table" data-testid="authors-table"> … </table>
      </div>
    </section>
  </main>
</div>
```

`frontend-kind` is how a test knows which frontend it is driving; everything else is identical.

### Navigation test ids

`nav-authors`, `nav-members`, `nav-catalog`, `nav-inventory`, `nav-reservations`, `nav-loans`, `nav-simulation`.

The Razor routes are `/authors`, `/members`, `/catalog`, `/inventory`, `/reservations`, `/loans`, `/simulation`.
The React app uses the same paths.

### Per-page test ids

| Page | Form fields | Submit | Table | Row |
|---|---|---|---|---|
| Authors | `author-first-name`, `author-last-name` | `author-submit` | `authors-table` | `author-row` |
| Members | `member-first-name`, `member-last-name` | `member-submit` | `members-table` | `member-row` |
| Catalog | `book-isbn`, `book-title`, `book-author` (select) | `book-submit` | `books-table` | `book-row` |
| Catalog — tagging | `tag-isbn` (select), `tag-value` | `tag-submit` | — | — |
| Inventory | `inventory-isbn` (select), `inventory-count` | `inventory-submit` | `inventory-table` | `inventory-row` |
| Inventory — losses | `lost-isbn` (select, from **inventory** not the catalog), `lost-count` | `lost-submit` | — | — |
| Reservations | `reservation-isbn` (select), `reservation-member` (select) | `reservation-submit` | `reservations-table` | `reservation-row` |
| Loans | `loan-isbn` (select), `loan-member` (select) | `loan-submit` | `loans-table` | `loan-row` |
| Simulation | `simulation-count` | `simulation-start` | — | — |

Every capability the API exposes is reachable from both frontends — including tagging a book and writing copies
off as lost, which each get a second form card (`tag-form`, `lost-form`) below the page's main one.

Additional ids:

- Authors: each row has a delete button `author-delete` (this is the action that 409s when the author has books).
- Loans: each open loan row has a return button `loan-return`.
- Simulation: `simulation-status`, `simulation-progress`, `simulation-succeeded`, `simulation-rejected`,
  `simulation-failed`, `simulation-stop`. `simulation-progress` goes on the inner **`.progress-bar`** — the element
  carrying the `width` — not on the `.progress` container, so a test can read the percentage off it.
- Every row carries `data-id` with its integer key (`data-id="12"`), and catalog rows also carry `data-isbn`.
  **Inventory rows are the exception** — `InventoryDetails` has no integer key of its own, so inventory rows carry
  only `data-isbn` and tests must key off that.

### Selects

Author, member, and ISBN pickers are `<select class="select">` populated from the API, each `<option>`'s `value`
being the integer id (or the ISBN string), so a test can select by value in either frontend.

## API

Both frontends use the same endpoints. Responses are camelCase JSON.

| Method | Path | Body | Success | Rejections |
|---|---|---|---|---|
| GET | `/api/authors` | — | 200 `AuthorDetails[]` | |
| POST | `/api/authors` | `{firstName,lastName}` | 201 `AuthorDetails` | |
| DELETE | `/api/authors/{id}` | — | 204 | 409 when the author still has books |
| GET | `/api/members` | — | 200 `MemberDetails[]` | |
| POST | `/api/members` | `{firstName,lastName}` | 201 `MemberDetails` | |
| GET | `/api/catalog/books` | — | 200 `BookDetails[]` | |
| POST | `/api/catalog/books` | `{isbn,title,authorId}` | 201 `BookDetails` | 422 unknown author |
| POST | `/api/catalog/books/{isbn}/tags` | `{tag}` | 201 | 404 unknown book, 409 duplicate tag |
| GET | `/api/inventory` | — | 200 `InventoryDetails[]` | |
| POST | `/api/inventory` | `{isbn,count}` | 201 `InventoryDetails` | 422 unknown title |
| POST | `/api/inventory/{isbn}/lost` | `{count}` | 200 `InventoryDetails` | 404, 422 more lost than held |
| GET | `/api/reservations` | — | 200 `ReservationDetails[]` | |
| POST | `/api/reservations` | `{isbn,memberId}` | 201 `ReservationDetails` | **422 when no copies available** |
| GET | `/api/loans` | — | 200 `LoanDetails[]` | |
| POST | `/api/loans` | `{isbn,memberId}` | 201 `LoanDetails` | 422 when no copies available |
| POST | `/api/loans/{id}/return` | `{}` | 200 `LoanDetails` | 404, 409 already returned |
| GET | `/api/simulation/status` | — | 200 `SimulationStatus` | |
| POST | `/api/simulation/start` | `{transactionCount}` | 202 `SimulationStatus` | 409 already running |
| POST | `/api/simulation/stop` | — | 200 `SimulationStatus` | |

Rejections come back as RFC 7807 `ProblemDetails`; show `title` and `detail` in the `data-testid="error"` message
block. A rejection is a normal outcome to display, not a crash.
