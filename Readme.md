# QuickGo Application

Plataforma de subastas inversas de transporte de carga. Backend en .NET 10 + PostgreSQL.

## Requisitos
- .NET SDK 10.0.401 (ver `backend/global.json`)
- Docker (para PostgreSQL)

## Puesta en marcha (backend)

```bash
# 1) Base de datos
cd infra
cp example.env .env        # completa POSTGRES_USER, POSTGRES_PASSWORD y POSTGRES_DB
docker compose up -d

# 2) Variables de la API
cd ../backend/quickgo.API
cp .env.example .env       # ajusta la cadena de conexión (mismos datos que infra/.env) y Jwt__Key

# 3) Primera migración (solo la primera vez; se sube al repo)
cd ..
dotnet tool install --global dotnet-ef        # si no lo tienes
dotnet ef migrations add Inicial --project quickgo.Infrastructure --startup-project quickgo.API --output-dir Data/Migrations

# 4) Ejecutar (aplica las migraciones al iniciar cuando MigrarAlIniciar=true)
dotnet run --project quickgo.API               # http://localhost:5010
dotnet test                                    # pruebas
```

## Identidad y seguridad (Integrante 1)

| Endpoint | Roles |
|---|---|
| `POST /api/empresas/registro` | público |
| `POST /api/auth/login` | público |
| `GET /api/catalogos/roles` | Admin |
| `POST /api/usuarios`, `GET /api/usuarios`, `PUT /api/usuarios/{id}`, `PATCH /api/usuarios/{id}/desactivar` | Admin |
| `POST /api/conductores`, `PUT /api/conductores/{id}` | Admin, Despachador |
| `GET /api/conductores`, `GET /api/conductores/{id}` | Admin, Despachador, Conductor (solo el suyo) |
| `GET /api/conductores/licencias-por-vencer?dias=30` | Admin, Despachador |

El JWT lleva los claims `userId`, `empresaId`, `rol` y `tipoEmpresa`. Ningún endpoint recibe `EmpresaId` en el cuerpo:
sale del token, y las entidades con `EmpresaId` (`IEmpresaScoped`) llevan un filtro global por empresa en el `DbContext`.

### Modo Dev (solo `ASPNETCORE_ENVIRONMENT=Development`)
Sin hacer login se puede probar enviando headers: `X-Empresa-Id`, `X-User-Id`, `X-Rol` y, opcional, `X-Tipo-Empresa`.
Ver ejemplos en `backend/quickgo.API/quickgo.API.http`. La documentación OpenAPI está en `/openapi/v1.json`.
