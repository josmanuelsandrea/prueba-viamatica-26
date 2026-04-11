# Viamatica — Sistema de gestión de caja

Sistema de turnos y caja registradora para un proveedor de servicios de internet. Incluye kiosk de autoservicio para clientes, panel de gestión de turnos y cajas, y módulo de atención al cajero.

---

## Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y corriendo
- Puertos **4200**, **5000**, **5433** y **6379** disponibles en tu máquina

---

## Levantar el proyecto

```bash
docker compose up --build
```

La primera vez descarga las imágenes base y compila el frontend y el backend. Puede tardar unos minutos.

Una vez que todo esté corriendo:

| Servicio   | URL                          |
|------------|------------------------------|
| Frontend   | http://localhost:4200        |
| Backend    | http://localhost:5000        |
| Swagger    | http://localhost:5000/swagger |
| Kiosk      | http://localhost:4200/kiosk  |
| Redis      | localhost:6379               |

---

## Usuarios semilla

| Usuario     | Contraseña    | Rol            |
|-------------|---------------|----------------|
| `admin001`  | `Admin1234`   | Administrador  |
| `gestor001` | `Gestor1234`  | Gestor         |
| `cajero001` | `Cajero1234`  | Cajero         |

---

## Flujo básico de uso

1. Inicia sesión como **admin001** y crea una caja desde **Cajas**.
2. Inicia sesión como **gestor001**, ve a **Gestión de Cajas** y asigna a `cajero001` a esa caja. Actívalo.
3. Desde el **kiosk** (`/kiosk`) un cliente ingresa su cédula, elige el tipo de atención y recibe su número de turno.
4. El gestor puede también crear turnos manualmente desde **Asignación de Turnos**.
5. Inicia sesión como **cajero001** y ve a **Atenciones** para ver la cola de tu caja y marcar cada turno como atendido.

---

## Detener el proyecto

```bash
docker compose down
```

Para detener y eliminar también la base de datos (datos incluidos):

```bash
docker compose down -v
```

---

## Variables de entorno

El archivo `docker-compose.yml` acepta las siguientes variables de entorno opcionales. Puedes sobreescribirlas creando un archivo `.env` en la raíz del proyecto:

```env
POSTGRES_DB=viamatica
POSTGRES_USER=viamatica_user
POSTGRES_PASSWORD=viamatica_pass
JWT_KEY=SuperSecretKey32CharactersMinimum!
ENCRYPTION_KEY=AES256Key32BytesLongExactly!!!
REDIS_CONNECTION_STRING=redis:6379
```

---

## Estructura del proyecto

```
Backend/        .NET 10 Web API — arquitectura DDD por capas
Frontend/       Angular 21 SPA — módulos con lazy loading
AI/             Prompts utilizados durante el desarrollo
db_init.sql     Script de inicialización de la base de datos
docker-compose.yml
```
