# inmobiliaria_lab_prog_2

# Sistema de Gestión Inmobiliaria - Reservas Temporales

> Aplicación web desarrollada en ASP.NET Core MVC para la gestión de alquileres temporarios, propietarios, inquilinos e inmuebles[cite: 1].

---

## 👥 Integrantes del Grupo

* **Exequiel Muñoz** - *boschieze6@gmail.com* - [@exeqm1](https://github.com/eexeqm1)

---

##  Modelado de Datos

A continuación se presenta el esquema del modelo de datos correspondiente a la aplicación según la narrativa del sistema

### Diagrama Entidad-Relación (DER)

    PROPIETARIO ||--o{ INMUEBLE : posee
    TIPO_INMUEBLE ||--o{ INMUEBLE : clasifica
    INQUILINO ||--o{ RESERVA : solicita
    INMUEBLE ||--o{ RESERVA : incluye
    RESERVA ||--o{ PAGO : genera
    USUARIO ||--o{ RESERVA : gestiona
    USUARIO ||--o{ PAGO : registra

    PROPIETARIO {
        int id PK
        string dni
        string nombre
        string apellido
        string telefono
        string email
        bool estado
    }

    INQUILINO {
        int id PK
        string dni
        string nombre
        string apellido
        string telefono
        string email
        bool estado
    }

    TIPO_INMUEBLE {
        int id PK
        string descripcion
    }

    INMUEBLE {
        int id PK
        string direccion
        int cupo
        int tipo_id FK
        string coordenadas
        decimal precio_dia
        int porcentaje_reserva
        bool disponible
        int propietario_id FK
        bool estado
    }

    RESERVA {
        int id PK
        int inmueble_id FK
        int inquilino_id FK
        date fecha_inicio
        date fecha_fin
        decimal monto_diario
        date fecha_fin_efectiva
        decimal multa
        int usuario_creador_id FK
        int usuario_finalizador_id FK
        bool estado
    }

    PAGO {
        int id PK
        int reserva_id FK
        string concepto
        datetime fecha_pago
        decimal importe
        int usuario_creador_id FK
        int usuario_anulador_id FK
        bool anulado
    }

    USUARIO {
        int id PK
        string nombre
        string apellido
        string email
        string password
        string rol
        string avatar
        bool estado
    }

### Instrucciones para Levantar la Base de Datos

Para inicializar la base de datos a partir del script SQL provisto en este repositorio, sigue estos pasos:

Abrir el gestor de base de datos:

Inicia tu gestor preferido (MySQL Workbench, phpMyAdmin, DBeaver o la consola de MySQL).

Cargar el script:

Abre el archivo database.sql ubicado en la raíz del repositorio.

Ejecutar las sentencias:

Ejecuta el script completo. La instrucción CREATE DATABASE IF NOT EXISTS inmobiliaria_db; creará el esquema y generará las tablas correspondientes junto con las claves foráneas y los datos iniciales de prueba[cite: 1].

Verificación:

Comprueba que la base de datos inmobiliaria_db contenga las tablas creadas y listas para operar con la aplicación.
