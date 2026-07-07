USE master;
GO
DROP DATABASE DB_try;
GO

CREATE DATABASE DB_try
COLLATE Cyrillic_General_CI_AS;
GO

USE DB_try;
GO

-- Роль (преподаватель, студент, деканат, системный администратор)
CREATE TABLE roles (
    role_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

-- Факультет
CREATE TABLE faculty (
    faculty_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE
);

-- Направление
CREATE TABLE directions (
    direction_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    faculty_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    FOREIGN KEY (faculty_id) REFERENCES faculty(faculty_id)
);

-- Группа
CREATE TABLE groups (
    group_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    direction_id INT NOT NULL,
    name VARCHAR(50) NOT NULL,
    FOREIGN KEY (direction_id) REFERENCES directions(direction_id)
);

-- Основная таблица пользователей
CREATE TABLE users (
    user_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    fio VARCHAR(255) NOT NULL,
    role_id INT NOT NULL,
    login VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    phone VARCHAR(20),
    FOREIGN KEY (role_id) REFERENCES roles(role_id)
);

-- Уникальный индекс для определния пользователей должен однозначно идентифицировать пользователя (рассылки, восстановление пароля)
-- Из минусов каждая вставка/изменение теперь требует проверки уникальности в индексе, от этого медленнее 
CREATE UNIQUE NONCLUSTERED INDEX I_Users_Email ON users(email) WHERE email IS NOT NULL;
GO

-- Связь студентов с группами (я исправила тот тупой наворот)
CREATE TABLE student_groups (
    student_id INT NOT NULL,
    group_id INT NOT NULL,
    PRIMARY KEY (student_id),
    FOREIGN KEY (student_id) REFERENCES users(user_id),
    FOREIGN KEY (group_id) REFERENCES groups(group_id)
);

-- Новости
CREATE TABLE news (
    news_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    news_title VARCHAR(255) NOT NULL,
    news_text NVARCHAR(MAX),
    publication_date DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- Тип пары (Лекция, Лабораторная, Практика)
CREATE TABLE descriptions (
    description_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(25) NOT NULL UNIQUE
);

-- Предметы (убрана привязка к преподавателю)
CREATE TABLE subjects (
    subject_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE
);

-- Связь преподавателей с предметами 
CREATE TABLE teacher_subjects (
    teacher_id INT NOT NULL,
    subject_id INT NOT NULL,
    PRIMARY KEY (teacher_id, subject_id),
    FOREIGN KEY (teacher_id) REFERENCES users(user_id),
    FOREIGN KEY (subject_id) REFERENCES subjects(subject_id)
);

-- Аудитории
CREATE TABLE classrooms (
    classroom_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

-- Таблица для хранения информации о времени пар
CREATE TABLE lesson_times (
    time_slot_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    lesson_number INT NOT NULL UNIQUE,
    time_start TIME NOT NULL,
    time_end TIME NOT NULL
);

-- Расписание (тут были исправления)
CREATE TABLE schedule (
    schedule_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    group_id INT NOT NULL,
    subject_id INT NOT NULL,
    teacher_id INT NOT NULL,
    classroom_id INT NOT NULL,
    day_of_week VARCHAR(15) NOT NULL,
    time_slot_id INT NOT NULL,
    week_type VARCHAR(10) NOT NULL DEFAULT N'Общая',
    description_id INT NOT NULL,
    FOREIGN KEY (group_id) REFERENCES groups(group_id),
    FOREIGN KEY (subject_id) REFERENCES subjects(subject_id),
    FOREIGN KEY (teacher_id) REFERENCES users(user_id),
    FOREIGN KEY (classroom_id) REFERENCES classrooms(classroom_id),
    FOREIGN KEY (time_slot_id) REFERENCES lesson_times(time_slot_id),
    FOREIGN KEY (description_id) REFERENCES descriptions(description_id),
    CONSTRAINT check_day_of_week CHECK (day_of_week IN (N'Понедельник', N'Вторник', N'Среда', N'Четверг', N'Пятница', N'Суббота')),
    CONSTRAINT check_week_type CHECK (week_type IN (N'Четная', N'Нечетная', N'Общая')),
    CONSTRAINT unique_schedule UNIQUE (group_id, subject_id, teacher_id, classroom_id, day_of_week, time_slot_id, week_type)
);

-- Журнал
CREATE TABLE journal (
    journal_id INTEGER IDENTITY(1,1) PRIMARY KEY,
    schedule_id INT NOT NULL,
    student_id INT NOT NULL,
    lesson_date DATE NOT NULL,
    grade INT CHECK (grade BETWEEN 2 AND 5),
    attendance VARCHAR(15) NOT NULL,
    
    FOREIGN KEY (schedule_id) REFERENCES schedule(schedule_id),
    FOREIGN KEY (student_id) REFERENCES users(user_id),
    
    CONSTRAINT unique_student_lesson UNIQUE (student_id, schedule_id, lesson_date),
    CONSTRAINT check_attendance_status CHECK (attendance IN (N'Присутствует', N'Отсутствует', N'Болеет'))
);
