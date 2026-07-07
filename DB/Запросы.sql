-- Выборка данных с фильтрацией, сортировкой
SELECT fio, email, phone FROM users
WHERE role_id = 2 AND phone IS NOT NULL
ORDER BY fio;

-- Удаление, изменение данных
DELETE FROM news WHERE news_id = 3;
SELECT * FROM news WHERE news_id = 3;

UPDATE users SET phone = N'89001231234' WHERE login = N'sokolov';
SELECT login, phone FROM users WHERE login = N'sokolov';

-- Выборка с группировкой (количество студентов в каждой группе)
SELECT g.name AS Группа, COUNT(sg.student_id) AS Количество
FROM groups g
LEFT JOIN student_groups sg ON g.group_id = sg.group_id
GROUP BY g.name
ORDER BY Количество DESC;

-- Выборка из нескольких связанных таблиц
-- левое (всех выводим с группой)
SELECT u.fio AS ФИО, roles.name AS Роль, g.name AS Группа
FROM users u
JOIN roles ON u.role_id = roles.role_id
LEFT JOIN student_groups sg ON u.user_id = sg.student_id
LEFT JOIN groups g ON sg.group_id = g.group_id
ORDER BY roles.name, u.fio;

-- правое (все группы и их студенты)
SELECT g.name AS Группа, u.fio AS Студент
FROM student_groups sg
RIGHT JOIN groups g ON sg.group_id = g.group_id
LEFT JOIN users u ON sg.student_id = u.user_id
ORDER BY g.name, u.fio;

-- пересечение (студенты с их информацией обучения)
SELECT u.fio AS ФИО, g.name AS Группа, d.name AS Направление, f.name AS Факультет
FROM users u
JOIN student_groups sg ON u.user_id = sg.student_id
JOIN groups g ON sg.group_id = g.group_id
JOIN directions d ON g.direction_id = d.direction_id
JOIN faculty f ON d.faculty_id = f.faculty_id
ORDER BY g.name, u.fio;