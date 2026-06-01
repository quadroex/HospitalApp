# Огляд бізнес-правил HospitalApp

Цей документ підготовлено за результатами read-only перевірки `information_schema` і `pg_catalog`. Схема бази даних не змінювалася.

## Правила, які вже контролює база даних

- Первинні ключі:
  - `departments(name)`
  - `doctors(passport)`
  - `patients(card_number)`
  - `rooms(number)`
  - `therapists(doctor_passport)`
  - `surgeons(doctor_passport)`
  - `visit_records(patient_card_number, visit_date, visit_time)`
  - `appointments(doctor_passport, patient_card_number, room_number)`
  - `doctor_consultations(doctor_passport, consultant_passport)`
- Обов'язковість полів через `NOT NULL` для всіх ключових і основних атрибутів. Необов'язкові поля: `middle_name`, `visit_records.complaints`.
- Довжини `varchar`:
  - паспорти, номери карток, номери кабінетів: до 20 символів;
  - назви відділень, ПІБ, спеціалізація, категорія: до 100 символів;
  - обладнання кабінету: до 255 символів.
- Зовнішні ключі між прийнятими таблицями моделі з натуральними і складеними ключами.
- `departments(name, head_passport)` посилається на `doctors(department_name, passport)` і є `DEFERRABLE INITIALLY DEFERRED`.
- `departments.floor > 0`.
- `therapists.districts_count >= 0`.
- `doctor_consultations.doctor_passport <> doctor_consultations.consultant_passport`.
- Унікальність `departments.head_passport`: один лікар не може бути завідувачем кількох відділень.
- Унікальність `doctors(department_name, passport)` для складеного FK завідувача.

## Правила, які після цієї фази контролює застосунок

- Текстові поля перевіряються до `INSERT`/`UPDATE`:
  - `passport`, `card_number`: 3-20 символів;
  - `first_name`, `last_name`: 2-50 символів;
  - `middle_name`: необов'язково, але якщо заповнено, 2-50 символів;
  - `department name`: 2-100 символів;
  - `specialization`, `surgeons.category`: 2-100 символів;
  - `rooms.number`: 1-20 символів;
  - `equipment_type`: 2-255 символів;
  - `complaints`: до 500 символів.
- Числові поля:
  - `departments.floor`: 1-200;
  - `therapists.districts_count`: 0-100.
- Дати:
  - `patients.birth_date`: від `1900-01-01` до сьогодні включно;
  - `visit_records.visit_date` у режимі додавання: завтра або пізніше.
- У режимі редагування старі PK-поля `visit_records` не блокують збереження скарг тільки через те, що дата візиту вже в минулому.
- `doctor_consultations`:
  - лікар не може консультувати сам себе;
  - зворотна пара `B -> A` блокується, якщо вже існує `A -> B`.
- `appointments`: точний дублікат перевіряється перед вставкою для дружнього повідомлення.
- `departments`: під час редагування завідувач має працювати у цьому ж відділенні.
- `doctors`: якщо лікар є завідувачем, зміна його `department_name` блокується дружнім повідомленням.

## Рекомендовані DB-обмеження, але не виконані

Файл `sql/business_constraints.sql` містить review-only SQL з можливими додатковими обмеженнями. Він не запускається застосунком і не був виконаний під час цієї фази.

Рекомендації:

- посилити `departments.floor` до `BETWEEN 1 AND 200`;
- посилити `therapists.districts_count` до `BETWEEN 0 AND 100`;
- додати унікальний індекс для симетричної заборони зворотних консультацій;
- додати базові обмеження довжин і мінімальних довжин для основних текстових полів;
- для `birth_date <= CURRENT_DATE` краще залишити перевірку на рівні застосунку, бо `CURRENT_DATE` у `CHECK` є часово залежною умовою.

## ON DELETE поведінка

- `appointments`:
  - лікар, пацієнт або кабінет не можуть бути видалені, якщо на них посилаються прийоми (`ON DELETE RESTRICT`).
- `doctors.department_name -> departments.name`:
  - відділення не можна видалити, якщо в ньому працюють лікарі (`ON DELETE RESTRICT`).
- `departments(name, head_passport) -> doctors(department_name, passport)`:
  - `ON DELETE NO ACTION`, перевірка відкладена завдяки deferrable constraint.
- `therapists`, `surgeons`, `visit_records`, `doctor_consultations` у поточній фактичній схемі мають `ON DELETE CASCADE` до батьківських записів.

Каскадне видалення в цій фазі не додавалося і не змінювалося. Якщо потрібно зробити всю модель строго обмежувальною без каскадів, це має бути окреме явне рішення щодо схеми.
