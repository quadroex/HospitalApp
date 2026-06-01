-- Review-only recommendations for HospitalApp business constraints.
-- Do not execute this script automatically from the application.
-- Review with the teacher/DB owner before applying anything to the accepted schema.

-- departments.floor is currently checked as floor > 0.
-- Recommended stricter educational rule:
ALTER TABLE departments
    ADD CONSTRAINT chk_departments_floor_range
    CHECK (floor BETWEEN 1 AND 200);

-- therapists.districts_count is currently checked as districts_count >= 0.
-- App-level validation uses 0..100 because existing/test data may allow 0.
ALTER TABLE therapists
    ADD CONSTRAINT chk_therapists_districts_count_range
    CHECK (districts_count BETWEEN 0 AND 100);

-- Self-consultation is already blocked in the current database.
-- Keep here as the intended business rule if rebuilding constraints later.
ALTER TABLE doctor_consultations
    ADD CONSTRAINT chk_doctor_consultations_not_self
    CHECK (doctor_passport <> consultant_passport);

-- Prevent both A -> B and B -> A from existing at the same time.
-- PostgreSQL supports expression indexes for this rule.
CREATE UNIQUE INDEX ux_doctor_consultations_unordered_pair
    ON doctor_consultations (
        LEAST(doctor_passport, consultant_passport),
        GREATEST(doctor_passport, consultant_passport)
    );

-- Basic length recommendations. The database already has varchar maximums;
-- these checks add minimum lengths that the application now validates too.
ALTER TABLE departments
    ADD CONSTRAINT chk_departments_name_length
    CHECK (char_length(name) BETWEEN 2 AND 100);

ALTER TABLE doctors
    ADD CONSTRAINT chk_doctors_text_lengths
    CHECK (
        char_length(passport) BETWEEN 3 AND 20
        AND char_length(last_name) BETWEEN 2 AND 100
        AND char_length(first_name) BETWEEN 2 AND 100
        AND (middle_name IS NULL OR char_length(middle_name) BETWEEN 2 AND 100)
        AND char_length(specialization) BETWEEN 2 AND 100
    );

ALTER TABLE patients
    ADD CONSTRAINT chk_patients_text_lengths
    CHECK (
        char_length(card_number) BETWEEN 3 AND 20
        AND char_length(last_name) BETWEEN 2 AND 100
        AND char_length(first_name) BETWEEN 2 AND 100
        AND (middle_name IS NULL OR char_length(middle_name) BETWEEN 2 AND 100)
    );

ALTER TABLE rooms
    ADD CONSTRAINT chk_rooms_text_lengths
    CHECK (
        char_length(number) BETWEEN 1 AND 20
        AND char_length(equipment_type) BETWEEN 2 AND 255
    );

ALTER TABLE surgeons
    ADD CONSTRAINT chk_surgeons_category_length
    CHECK (char_length(category) BETWEEN 2 AND 100);

-- Lower bound for birth dates is stable and safe as a CHECK.
ALTER TABLE patients
    ADD CONSTRAINT chk_patients_birth_date_min
    CHECK (birth_date >= DATE '1900-01-01');

-- Do not add CHECK (birth_date <= CURRENT_DATE) without careful review:
-- CURRENT_DATE is time-dependent, so rows valid today may become evaluated
-- differently by future operations. HospitalApp keeps this rule app-level.
