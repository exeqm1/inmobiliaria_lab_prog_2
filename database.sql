-- Creación y selección de la base de datos
CREATE DATABASE IF NOT EXISTS inmobiliaria_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE inmobiliaria_db;

-- 1. Tabla Propietarios
CREATE TABLE IF NOT EXISTS Propietarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    dni VARCHAR(20) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    telefono VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    estado BIT NOT NULL DEFAULT 1,
    INDEX idx_propietario_dni (dni),
    INDEX idx_propietario_estado (estado)
);

-- 2. Tabla Inquilinos
CREATE TABLE IF NOT EXISTS Inquilinos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    dni VARCHAR(20) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    telefono VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    estado BIT NOT NULL DEFAULT 1,
    INDEX idx_inquilino_dni (dni),
    INDEX idx_inquilino_estado (estado)
);

-- 3. Tabla Tipos_Inmueble (ABM de catálogo)
CREATE TABLE IF NOT EXISTS Tipos_Inmueble (
    id INT AUTO_INCREMENT PRIMARY KEY,
    descripcion VARCHAR(100) NOT NULL,
    estado BIT NOT NULL DEFAULT 1
);

-- 4. Tabla Usuarios (Autenticación y Roles)
CREATE TABLE IF NOT EXISTS Usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    rol VARCHAR(50) NOT NULL,       -- 'Administrador' o 'Empleado'
    avatar VARCHAR(255) NULL,
    estado BIT NOT NULL DEFAULT 1
);

-- 5. Tabla Inmuebles
CREATE TABLE IF NOT EXISTS Inmuebles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    direccion VARCHAR(200) NOT NULL,
    cupo INT NOT NULL,
    tipo_id INT NOT NULL,
    coordenadas VARCHAR(100) NULL,
    precio_dia DECIMAL(12, 2) NOT NULL,
    porcentaje_reserva INT NOT NULL,
    portada VARCHAR(255) NULL,         -- Imagen principal
    disponible BIT NOT NULL DEFAULT 1,    -- Suspensión temporal de oferta
    propietario_id INT NOT NULL,
    estado BIT NOT NULL DEFAULT 1,       -- Baja lógica general
    CONSTRAINT fk_inmueble_tipo FOREIGN KEY (tipo_id) REFERENCES Tipos_Inmueble(id),
    CONSTRAINT fk_inmueble_propietario FOREIGN KEY (propietario_id) REFERENCES Propietarios(id),
    INDEX idx_inmueble_disp_estado (disponible, estado),
    INDEX idx_inmueble_propietario (propietario_id)
);

-- 5.1. Galería de Fotos adicionales del Inmueble
CREATE TABLE IF NOT EXISTS Fotos_Inmueble (
    id INT AUTO_INCREMENT PRIMARY KEY,
    inmueble_id INT NOT NULL,
    url_foto VARCHAR(255) NOT NULL,
    CONSTRAINT fk_fotos_inmueble FOREIGN KEY (inmueble_id) REFERENCES Inmuebles(id) ON DELETE CASCADE
);

-- 6. Tabla Reservas
CREATE TABLE IF NOT EXISTS Reservas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    inmueble_id INT NOT NULL,
    inquilino_id INT NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,            -- Fecha de fin original pactada (inmutable)
    monto_diario DECIMAL(12, 2) NOT NULL,
    fecha_fin_efectiva DATE NULL,       -- Registrada en rescisión anticipada
    multa DECIMAL(12, 2) NULL DEFAULT 0,
    usuario_creador_id INT NOT NULL,    -- Auditoría
    usuario_finalizador_id INT NULL,    -- Auditoría
    estado BIT NOT NULL DEFAULT 1,       -- 1: Activa/Vigente/Cumplida, 0: Baja lógica
    CONSTRAINT fk_reserva_inmueble FOREIGN KEY (inmueble_id) REFERENCES Inmuebles(id),
    CONSTRAINT fk_reserva_inquilino FOREIGN KEY (inquilino_id) REFERENCES Inquilinos(id),
    CONSTRAINT fk_reserva_usuario_creador FOREIGN KEY (usuario_creador_id) REFERENCES Usuarios(id),
    CONSTRAINT fk_reserva_usuario_finalizador FOREIGN KEY (usuario_finalizador_id) REFERENCES Usuarios(id),
    INDEX idx_reserva_rango (inmueble_id, fecha_inicio, fecha_fin, estado)
);

-- 7. Tabla Pagos
CREATE TABLE IF NOT EXISTS Pagos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    reserva_id INT NOT NULL,
    concepto VARCHAR(150) NOT NULL,     -- Seña, Saldo, Multa terminación anticipada
    fecha_pago DATETIME NOT NULL,       -- Inmutable al editar
    importe DECIMAL(12, 2) NOT NULL,    -- Inmutable al editar
    usuario_creador_id INT NOT NULL,    -- Auditoría
    usuario_anulador_id INT NULL,       -- Auditoría
    anulado BIT NOT NULL DEFAULT 0,     -- Baja lógica / Anulación visible
    CONSTRAINT fk_pago_reserva FOREIGN KEY (reserva_id) REFERENCES Reservas(id),
    CONSTRAINT fk_pago_usuario_creador FOREIGN KEY (usuario_creador_id) REFERENCES Usuarios(id),
    CONSTRAINT fk_pago_usuario_anulador FOREIGN KEY (usuario_anulador_id) REFERENCES Usuarios(id),
    INDEX idx_pago_reserva (reserva_id, anulado)
);

-- ==========================================================
-- DATOS SEMILLA PARA PRUEBAS Y CÁTEDRA
-- ==========================================================
SET NAMES utf8mb4;

SET FOREIGN_KEY_CHECKS = 0;
TRUNCATE TABLE Pagos;
TRUNCATE TABLE Reservas;
TRUNCATE TABLE Fotos_Inmueble;
TRUNCATE TABLE Inmuebles;
TRUNCATE TABLE Inquilinos;
TRUNCATE TABLE Propietarios;
TRUNCATE TABLE Usuarios;
TRUNCATE TABLE Tipos_Inmueble;
SET FOREIGN_KEY_CHECKS = 1;

-- Catálogo de tipos de inmueble
INSERT INTO Tipos_Inmueble (id, descripcion, estado) VALUES
(1, 'Casa', 1),
(2, 'Departamento', 1),
(3, 'Monoambiente', 1),
(4, 'Cabaña', 1),
(5, 'Loft', 1);

-- Usuarios (PBKDF2 SHA256, 100.000 iteraciones)
-- Admin:             admin@inmobiliaria.com / admin123
-- Empleado:          empleado@inmobiliaria.com / empleado123
-- Sofía (admin):     sofia.ramirez@inmobiliaria.com / admin123
-- Martín/Valentina/Nicolás (empleados): empleado123
INSERT INTO Usuarios (id, nombre, apellido, email, password, rol, avatar, estado) VALUES
(1, 'Admin', 'Principal', 'admin@inmobiliaria.com', '100000.hKgNrDFRCMkPvMplqT9sRg==.QTHPhoLDmNC8c7b89zPzuV+XCtfdOsCkwPgEpynEjro=', 'Administrador', NULL, 1),
(2, 'Juan', 'Empleado', 'empleado@inmobiliaria.com', '100000.stc8f6kLcfaP5YmiciRBNQ==.ZYXyG00Oylb79ILW4QJH3m04ngCai57RbNC2MgG6iq8=', 'Empleado', NULL, 1),
(3, 'Sofía', 'Ramírez', 'sofia.ramirez@inmobiliaria.com', '100000.04NdV8kPuzb0gGSb7Nvc8w==.ncVm65/w3mgez/35qHpBDjI3TKkGQGDXWU9TFg4X6ow=', 'Administrador', NULL, 1),
(4, 'Martín', 'Gómez', 'martin.gomez@inmobiliaria.com', '100000.kx3GrQC4Ja4opyEHDidvmg==.tUWcri1Ns+6DFU+J1GRhC1ezx0F2GeKF8mMqzSIw43k=', 'Empleado', NULL, 1),
(5, 'Valentina', 'López', 'valentina.lopez@inmobiliaria.com', '100000.ri43EbxdgY1j/1XZ468kKw==.c71cVHD1NtB5iIEpaK1g6iBBd00CVPd2Uv3hGZMW4VY=', 'Empleado', NULL, 1),
(6, 'Nicolás', 'Fernández', 'nicolas.fernandez@inmobiliaria.com', '100000.fFDGrVUoEhTY/rMLelvE6Q==.0H+d7jShSFDmagalslD3qoAyOeeQ+6K0X1s/GwBSpIU=', 'Empleado', NULL, 1);

-- Propietarios
INSERT INTO Propietarios (id, dni, nombre, apellido, telefono, email, estado) VALUES
(1, '30111222', 'Diego Armando', 'Maradona', '2664112233', 'diegote10@gmail.com', 1),
(2, '35444555', 'Lionel', 'Scaloni', '2664445566', 'lioscaloneta@gmail.com', 1),
(3, '27654321', 'Marta', 'Sánchez', '2664123456', 'marta.sanchez@gmail.com', 1),
(4, '28987654', 'Roberto', 'Díaz', '2664234567', 'roberto.diaz@gmail.com', 1),
(5, '31234567', 'Ana', 'Martínez', '2664345678', 'ana.martinez@gmail.com', 1),
(6, '33456789', 'Pablo', 'Romero', '2664456789', 'pablo.romero@gmail.com', 1),
(7, '35678901', 'Laura', 'Benítez', '2664567890', 'laura.benitez@gmail.com', 1),
(8, '37890123', 'Gustavo', 'Acosta', '2664678901', 'gustavo.acosta@gmail.com', 1),
(9, '39012345', 'Silvia', 'Medina', '2664789012', 'silvia.medina@gmail.com', 1),
(10, '41234567', 'Hernán', 'Castro', '2664890123', 'hernan.castro@gmail.com', 1);

-- Inquilinos
INSERT INTO Inquilinos (id, dni, nombre, apellido, telefono, email, estado) VALUES
(1, '38777888', 'Carlos', 'Rodríguez', '2664778899', 'carlos.rodriguez@gmail.com', 1),
(2, '40999000', 'Lucía', 'Fernández', '2664001122', 'lucia.fernandez@gmail.com', 1),
(3, '41555111', 'María', 'González', '2664111222', 'maria.gonzalez@gmail.com', 1),
(4, '42777222', 'Javier', 'Paredes', '2664222333', 'javier.paredes@gmail.com', 1),
(5, '43999333', 'Florencia', 'Ríos', '2664333444', 'florencia.rios@gmail.com', 1),
(6, '44111444', 'Diego', 'Morales', '2664444555', 'diego.morales@gmail.com', 1),
(7, '45222555', 'Camila', 'Herrera', '2664555666', 'camila.herrera@gmail.com', 1),
(8, '46333666', 'Facundo', 'Silva', '2664666777', 'facundo.silva@gmail.com', 1),
(9, '47444777', 'Julieta', 'Navarro', '2664777888', 'julieta.navarro@gmail.com', 1),
(10, '48555888', 'Tomás', 'Aguirre', '2664888999', 'tomas.aguirre@gmail.com', 1),
(11, '49666999', 'Paula', 'Sosa', '2664999000', 'paula.sosa@gmail.com', 1),
(12, '50777000', 'Lucas', 'Peralta', '2664000111', 'lucas.peralta@gmail.com', 1);

-- Inmuebles (con portada en /uploads/inmuebles/demo/)
INSERT INTO Inmuebles (id, direccion, cupo, tipo_id, coordenadas, precio_dia, porcentaje_reserva, portada, disponible, propietario_id, estado) VALUES
(1, 'Av. Illia 450, San Luis', 4, 2, '-33.2981,-66.3356', 35000.00, 30, '/uploads/inmuebles/demo/inmueble-01-a.jpg', 1, 1, 1),
(2, 'Los Eucaliptos 123, Potrero de los Funes', 6, 4, '-33.2241,-66.2312', 60000.00, 50, '/uploads/inmuebles/demo/inmueble-02-a.jpg', 1, 2, 1),
(3, 'Ruta 3 Km 12, San Luis', 8, 1, '-33.3150,-66.3400', 45000.00, 30, '/uploads/inmuebles/demo/inmueble-03-a.jpg', 1, 3, 1),
(4, 'Av. Fuerza Aérea 780, San Luis', 2, 3, '-33.3010,-66.3370', 22000.00, 100, '/uploads/inmuebles/demo/inmueble-04-a.jpg', 1, 4, 1),
(5, 'Chacabuco 560, San Luis', 6, 2, '-33.3065,-66.3345', 38000.00, 50, '/uploads/inmuebles/demo/inmueble-05-a.jpg', 1, 5, 1),
(6, 'Ruta 20 Km 5, Juana Koslay', 4, 4, '-33.2830,-66.3390', 52000.00, 50, '/uploads/inmuebles/demo/inmueble-06-a.jpg', 1, 6, 1),
(7, 'Bolívar 145, San Luis', 3, 5, '-33.3102,-66.3310', 28000.00, 30, '/uploads/inmuebles/demo/inmueble-07-a.jpg', 0, 7, 1),
(8, 'Los Aromos 77, El Volcán', 10, 1, '-33.2440,-66.2390', 68000.00, 40, '/uploads/inmuebles/demo/inmueble-08-a.jpg', 1, 8, 1),
(9, 'Pedernera 230, San Luis', 4, 2, '-33.2975,-66.3305', 36000.00, 30, '/uploads/inmuebles/demo/inmueble-09-a.jpg', 1, 9, 1),
(10, 'San Martín 1020, San Luis', 5, 2, '-33.3030,-66.3380', 40000.00, 50, '/uploads/inmuebles/demo/inmueble-10-a.jpg', 1, 10, 1),
(11, 'Rincón del Este 15, Potrero de los Funes', 6, 4, '-33.2285,-66.2320', 58000.00, 50, '/uploads/inmuebles/demo/inmueble-11-a.jpg', 1, 3, 1),
(12, 'Colón 890, San Luis', 2, 3, '-33.3085,-66.3360', 24000.00, 100, '/uploads/inmuebles/demo/inmueble-12-a.jpg', 1, 4, 1);

-- Galería (2 fotos adicionales por inmueble)
INSERT INTO Fotos_Inmueble (inmueble_id, url_foto) VALUES
(1, '/uploads/inmuebles/demo/inmueble-01-b.jpg'), (1, '/uploads/inmuebles/demo/inmueble-01-c.jpg'),
(2, '/uploads/inmuebles/demo/inmueble-02-b.jpg'), (2, '/uploads/inmuebles/demo/inmueble-02-c.jpg'),
(3, '/uploads/inmuebles/demo/inmueble-03-b.jpg'), (3, '/uploads/inmuebles/demo/inmueble-03-c.jpg'),
(4, '/uploads/inmuebles/demo/inmueble-04-b.jpg'), (4, '/uploads/inmuebles/demo/inmueble-04-c.jpg'),
(5, '/uploads/inmuebles/demo/inmueble-05-b.jpg'), (5, '/uploads/inmuebles/demo/inmueble-05-c.jpg'),
(6, '/uploads/inmuebles/demo/inmueble-06-b.jpg'), (6, '/uploads/inmuebles/demo/inmueble-06-c.jpg'),
(7, '/uploads/inmuebles/demo/inmueble-07-b.jpg'), (7, '/uploads/inmuebles/demo/inmueble-07-c.jpg'),
(8, '/uploads/inmuebles/demo/inmueble-08-b.jpg'), (8, '/uploads/inmuebles/demo/inmueble-08-c.jpg'),
(9, '/uploads/inmuebles/demo/inmueble-09-b.jpg'), (9, '/uploads/inmuebles/demo/inmueble-09-c.jpg'),
(10, '/uploads/inmuebles/demo/inmueble-10-b.jpg'), (10, '/uploads/inmuebles/demo/inmueble-10-c.jpg'),
(11, '/uploads/inmuebles/demo/inmueble-11-b.jpg'), (11, '/uploads/inmuebles/demo/inmueble-11-c.jpg'),
(12, '/uploads/inmuebles/demo/inmueble-12-b.jpg'), (12, '/uploads/inmuebles/demo/inmueble-12-c.jpg');

-- Reservas: vigentes, futuras, rescindidas (multa) y una cancelada
INSERT INTO Reservas (id, inmueble_id, inquilino_id, fecha_inicio, fecha_fin, monto_diario, fecha_fin_efectiva, multa, usuario_creador_id, usuario_finalizador_id, estado) VALUES
(1, 1, 3, '2026-10-20', '2026-10-30', 35000.00, NULL, 0.00, 1, NULL, 1),
(2, 2, 4, '2026-11-05', '2026-11-12', 60000.00, NULL, 0.00, 2, NULL, 1),
(3, 3, 5, '2026-11-15', '2026-11-22', 45000.00, NULL, 0.00, 3, NULL, 1),
(4, 4, 6, '2026-12-01', '2026-12-05', 22000.00, NULL, 0.00, 1, NULL, 1),
(5, 5, 7, '2026-12-10', '2026-12-18', 38000.00, NULL, 0.00, 2, NULL, 1),
(6, 6, 8, '2026-11-20', '2026-11-25', 52000.00, NULL, 0.00, 4, NULL, 1),
(7, 8, 9, '2026-12-28', '2027-01-05', 68000.00, NULL, 0.00, 3, NULL, 1),
(8, 9, 10, '2026-11-01', '2026-11-06', 36000.00, NULL, 0.00, 1, NULL, 1),
(9, 10, 11, '2026-12-15', '2026-12-20', 40000.00, NULL, 0.00, 2, NULL, 1),
(10, 11, 12, '2027-01-10', '2027-01-17', 58000.00, NULL, 0.00, 4, NULL, 1),
(11, 12, 3, '2026-10-05', '2026-10-12', 24000.00, '2026-10-09', 36000.00, 1, 1, 1),
(12, 1, 4, '2026-09-01', '2026-09-10', 35000.00, '2026-09-06', 70000.00, 1, 1, 1),
(13, 6, 5, '2026-09-15', '2026-09-20', 52000.00, '2026-09-18', 52000.00, 2, 2, 1),
(14, 9, 6, '2026-10-01', '2026-10-03', 36000.00, NULL, 0.00, 3, NULL, 0),
(15, 2, 7, '2026-11-25', '2026-11-30', 60000.00, NULL, 0.00, 2, NULL, 1),
(16, 5, 8, '2027-01-05', '2027-01-12', 38000.00, NULL, 0.00, 4, NULL, 1);

-- Pagos (incluye una seña anulada para mostrar el filtro)
INSERT INTO Pagos (reserva_id, concepto, fecha_pago, importe, usuario_creador_id, usuario_anulador_id, anulado) VALUES
(1, 'Seña', '2026-10-15 10:00:00', 100000.00, 1, NULL, 0),
(1, 'Saldo', '2026-10-28 10:00:00', 250000.00, 1, NULL, 0),
(2, 'Seña', '2026-11-01 10:00:00', 200000.00, 2, NULL, 0),
(3, 'Seña', '2026-11-10 10:00:00', 150000.00, 3, NULL, 0),
(4, 'Total', '2026-11-25 10:00:00', 88000.00, 1, NULL, 0),
(5, 'Seña', '2026-12-01 10:00:00', 100000.00, 2, NULL, 0),
(5, 'Saldo', '2026-12-19 10:00:00', 144000.00, 2, NULL, 0),
(6, 'Seña', '2026-11-15 10:00:00', 150000.00, 4, NULL, 0),
(7, 'Seña', '2026-12-20 10:00:00', 200000.00, 3, NULL, 0),
(8, 'Total', '2026-10-30 10:00:00', 180000.00, 1, NULL, 0),
(9, 'Seña', '2026-12-10 10:00:00', 120000.00, 2, NULL, 0),
(9, 'Saldo', '2026-12-21 10:00:00', 80000.00, 2, NULL, 0),
(10, 'Seña', '2027-01-05 10:00:00', 200000.00, 4, NULL, 0),
(11, 'Multa por rescisión', '2026-10-09 10:00:00', 36000.00, 1, NULL, 0),
(12, 'Multa por rescisión', '2026-09-06 10:00:00', 70000.00, 1, NULL, 0),
(13, 'Multa por rescisión', '2026-09-18 10:00:00', 52000.00, 2, NULL, 0),
(15, 'Seña', '2026-11-20 10:00:00', 120000.00, 2, NULL, 0),
(16, 'Seña', '2027-01-02 10:00:00', 150000.00, 4, NULL, 0),
(3, 'Seña', '2026-11-08 10:00:00', 50000.00, 3, 1, 1);
