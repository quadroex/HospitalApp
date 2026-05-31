# HospitalApp Demo Plan

## Purpose

HospitalApp is a simple C# WinForms educational application for working with a PostgreSQL hospital database. The application demonstrates table CRUD forms, foreign-key input through ComboBox controls, parameterized SQL queries, and a dedicated transactional form for adding a department together with its head doctor.

## Main Forms

- `MainMenuForm`: main navigation menu.
- `TableForm`: universal CRUD form with `DataGridView`.
- `RowEditForm`: add/edit dialog for ordinary table rows.
- `DepartmentWithHeadForm`: dedicated form for adding a new department and its head doctor in one deferred-constraint transaction.
- `QueriesForm`: query menu.
- `QueryParameterForm`: parameter input dialog for queries.
- `QueryResultForm`: read-only query result grid.

## Table CRUD Forms

- Departments: `departments`
- Doctors: `doctors`
- Therapists: `therapists`
- Surgeons: `surgeons`
- Patients: `patients`
- Visit records: `visit_records`
- Rooms: `rooms`
- Appointments: `appointments`
- Doctor consultations: `doctor_consultations`

## Query Demo Parameters

The following values were checked against the current demo database state with read-only `SELECT` queries.

| # | Query title | Recommended parameter value | Expected rows | Lab requirement satisfied |
|---|---|---:|---:|---|
| 1 | Лікарі заданого відділення | `department_name = Терапія` | 2 | Parameterized query using `doctors` and `departments`. |
| 2 | Пацієнти з візитами після заданої дати | `start_date = 1900-01-01` | 5 | Parameterized query using `patients` and `visit_records`. |
| 3 | Прийоми у кабінетах із заданим обладнанням | `equipment_text = стіл` | 3 | Parameterized query using `appointments`, `doctors`, `patients`, and `rooms`. |
| 4 | Пацієнти лікарів заданої спеціалізації | `specialization_text = Хірург` | 2 | Parameterized query using `patients`, `appointments`, and `doctors`. |
| 5 | Пацієнти старші за заданий вік із записами візитів | `min_age = 30` | 3 | Parameterized aggregate query using `patients` and `visit_records`. |
| 6 | Лікарі з таким самим набором пацієнтів, як заданий лікар | `doctor_passport = 222` | 1 | Set-comparison query using `EXCEPT` and `NOT EXISTS`. |
| 7 | Пари лікарів з однаковою множиною пацієнтів | no parameters | 1 | Set-comparison query using pair comparison, `EXCEPT`, and `NOT EXISTS`. |

## Set/Multiple Comparison Queries

Query 6 compares the set of patients assigned to one selected doctor with the set of patients assigned to every other doctor. For the current data, use `doctor_passport = 222`. It returns doctor `666`, because doctors `222` and `666` have the same non-empty patient set in `appointments`.

Query 7 searches all doctor pairs and returns pairs with identical non-empty patient sets. For the current data it returns:

| Doctor 1 | Doctor 2 |
|---|---|
| `222` - Лисенко Вікторія Олегівна | `666` - Петренко Наталія Олександрівна |

Both set-comparison queries satisfy the lab requirement for queries with set or multiple comparisons because they compare whole result sets, not only single scalar values.

## Manual GUI Test Checklist

1. Open the application and verify that the main menu appears.
2. Open each table form from the main menu.
3. Verify that every table form loads data into a `DataGridView`.
4. Open Add/Edit dialogs and verify that foreign-key fields use ComboBox controls.
5. Add, edit, and delete a harmless test patient manually.
6. Open the query menu and run all 7 queries using the parameters listed above.
7. Test the dedicated department-with-head form only if the user explicitly wants to insert a temporary visible row.

## Defense Notes

- CRUD operations use parameterized `NpgsqlCommand` values.
- Foreign keys are selected through ComboBox controls populated from referenced tables.
- Query results are read-only in `DataGridView`.
- Adding a brand-new department is handled by `DepartmentWithHeadForm`, because the database has a cyclic deferrable relation between `departments` and `doctors`.
- The department-with-head insert uses one transaction with deferred constraints.
