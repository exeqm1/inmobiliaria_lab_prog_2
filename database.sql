-- Creación y selección de la base de datos
CREATE DATABASE IF NOT EXISTS inmobiliaria_db;
USE inmobiliaria_db;

CREATE TABLE IF NOT EXISTS Propietarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    Telefono VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Estado BIT NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Inquilinos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    Telefono VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Estado BIT NOT NULL DEFAULT 1
);

INSERT INTO Propietarios (Dni, Nombre, Apellido, Telefono, Email, Estado) VALUES
('30111222', 'Diego Armando', 'Maradona', '2664112233', 'diegote10@gmail.com', 1),
('35444555', 'Lionel', 'Scaloni', '2664445566', 'lioscaloneta@gmail.com', 1);

INSERT INTO Inquilinos (Dni, Nombre, Apellido, Telefono, Email, Estado) VALUES
('38777888', 'Carlos', 'Rodriguez', '2664778899', 'carlos.lopezrodr@gmail.com', 1),
('40999000', 'Lucia', 'Fernández', '2664001122', 'lucia.fernandez@gmail.com', 1);