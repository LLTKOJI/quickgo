# QuickGo

Plataforma de **subastas inversas de transporte de carga** para Honduras. Una empresa *Contratante* publica una carga con un precio máximo y las empresas *Transportistas* pujan a la baja; al adjudicar se crea el envío en la empresa ganadora.

Cada empresa trabaja aislada de las demás (multi-empresa): el `empresaId` sale siempre del token JWT y ninguna consulta cruza datos entre empresas.

## Estructura del repositorio

```
quickgo/
├── backend/        API en .NET 10 (arquitectura por capas) + pruebas  → ver backend/README.md
├── infra/          docker-compose con PostgreSQL para desarrollo
├── .gitignore      qué NO se sube (bin, obj, .env, claves…)
└── README.md       este archivo
```

> El frontend (React) aún no está en este repositorio.

## Tecnologías

| Capa | Tecnología |
|---|---|
| API | .NET 10, ASP.NET Core (controllers), FluentValidation |
| Datos | PostgreSQL 16, Entity Framework Core 10 (migraciones) |
| Seguridad | JWT Bearer, hash PBKDF2, roles por endpoint |
| Docs de la API | OpenAPI + Scalar (solo en Development) |
| Pruebas | xUnit + EF Core InMemory, y un script de pruebas HTTP de punta a punta |

## Inicio rápido

Requisitos: **.NET SDK 10.0.401** (fijado en `backend/global.json`) y **Docker**.

```bash
# 1) Base de datos
cd infra
cp .env.example .env            # Windows: copy .env.example .env
docker compose up -d

# 2) Variables de la API
cd ../backend/quickgo.API
cp .env.example .env            # Windows: copy .env.example .env
#   → en producción cambia Jwt__Key por una clave propia de 32+ caracteres

# 3) Ejecutar (aplica las migraciones al iniciar)
cd ..
dotnet run --project quickgo.API
```

- API: <http://localhost:5010>
- Salud: <http://localhost:5010/health>
- Documentación interactiva (solo Development): <http://localhost:5010/scalar/v1>
- Especificación OpenAPI: <http://localhost:5010/openapi/v1.json>

Las instrucciones completas, la lista de endpoints y cómo correr las pruebas están en **[backend/README.md](backend/README.md)**.

## Estado del proyecto

| Módulo | Historias | Estado |
|---|---|---|
| Identidad, seguridad y conductores | B1-01 … B1-05 | ✅ Completo |
| Flota y documentos | B2-01, B2-02, B2-03 | ✅ Completo |
| Asignaciones vehículo–conductor | B2-04 | ⚠️ Falta `GET /api/conductores/{id}/asignaciones` |
| Docker de la API | B2-05 | ⏳ Pendiente (hoy solo está PostgreSQL en `infra/`) |
| Catálogos, clientes, direcciones | B3-01, B3-02, B3-03 | ✅ Completo |
| Facturación y pagos | B3-04 | ✅ Completo |
| CI/CD | B3-05 | ⏳ Pendiente |
| Envíos, cotizaciones, seguimiento | B4-01 … B4-05 | ⏳ Pendiente |
| Subastas: publicar, cancelar, mis subastas, cierre | B5-01, B5-02 | ✅ Completo |
| Subastas: pujar, mejor oferta, adjudicar | B5-03, B5-04, B5-05 | ⏳ Pendiente |

## Flujo de trabajo del equipo

- Una rama por historia: `feature/B#-##`, PR revisado por otro integrante y *squash* a `develop`/`main`.
- Todo endpoint toma la empresa del token; nunca del cuerpo de la petición.
- Cada historia incluye al menos una prueba con dos empresas para comprobar el aislamiento.
- **Nunca** subas archivos `.env`, claves ni contraseñas: están en el `.gitignore`. Sube solo los `*.env.example`.
