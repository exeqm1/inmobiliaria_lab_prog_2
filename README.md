# Sistema de Gestión Inmobiliaria - Reservas Temporales

> Aplicación web desarrollada en ASP.NET Core (.NET 10) MVC con MySQL para la gestión de alquileres temporarios, propietarios, inquilinos, inmuebles, reservas y pagos con doble auditoría.

---

## 👥 Integrantes del Grupo

* **Exequiel Muñoz** - *boschieze6@gmail.com* - [@exeqm1](https://github.com/eexeqm1)

---

## 📐 Modelado y Diseño del Sistema

A continuación se presentan los esquemas y diagramas correspondientes a la aplicación según la narrativa y reglas de negocio del sistema:

### 1. Diagrama Entidad-Relación (DER)
Representa la estructura física y lógica de la base de datos relacional MySQL, con notación Crow's Foot estricta.

![Diagrama Entidad-Relación](docs/diagrama-er.png)

*Archivos fuente editables disponibles en [`docs/diagrama-er.drawio`](docs/diagrama-er.drawio) y [`docs/diagrama-er.mmd`](docs/diagrama-er.mmd).*

---

### 2. Diagrama de Clases UML (Dominio Enriquecido)
Representa el modelo orientado a objetos en memoria (C#), con propiedades calculadas, métodos de dominio (`CalcularMulta`, `SaldoPendiente`, `TotalPagado`, `CantidadDias`), tipos de C#, navegabilidad y multiplicidades de asociación.

![Diagrama de Clases UML](docs/diagrama-clases.png)

*Archivos fuente editables disponibles en [`docs/diagrama-clases.drawio`](docs/diagrama-clases.drawio), [`docs/diagrama-clases.puml`](docs/diagrama-clases.puml) y [`docs/diagrama-clases.mmd`](docs/diagrama-clases.mmd).*

---

## 🗄️ Esquema de Entidades (MySQL)

```
PROPIETARIOS ||--o{ INMUEBLES : posee
TIPOS_INMUEBLE ||--o{ INMUEBLES : clasifica
INQUILINOS ||--o{ RESERVAS : solicita
INMUEBLES ||--o{ RESERVAS : incluye
INMUEBLES ||--o{ FOTOS_INMUEBLE : contiene
RESERVAS ||--o{ PAGOS : genera
USUARIOS ||--o{ RESERVAS : gestiona (creador / finalizador)
USUARIOS ||--o{ PAGOS : registra / anula
```

---

## 🚀 Instrucciones para Levantar la Base de Datos

Para inicializar la base de datos a partir del script SQL provisto en este repositorio, sigue estos pasos:

1. **Abrir el gestor de base de datos:**
   Inicia tu gestor preferido (MySQL Workbench, phpMyAdmin, DBeaver o la consola de MySQL).

2. **Cargar el script:**
   Abre el archivo `database.sql` ubicado en la raíz del repositorio.

3. **Ejecutar las sentencias:**
   Ejecuta el script completo. La instrucción `CREATE DATABASE IF NOT EXISTS inmobiliaria_db;` creará el esquema y generará las tablas correspondientes junto con las claves foráneas y los datos iniciales de prueba.

4. **Verificación:**
   Comprueba que la base de datos `inmobiliaria_db` contenga las tablas creadas y listas para operar con la aplicación.
