# TwitterUala — Arquitectura a alto nivel

Resumen
- TwitterUala es una Web API en .NET 8 que implementa un microblogging simple.  
- Arquitectura monolítica con capas lógicas: API (Controllers), Servicios, Dominio (Entities + DbContext) y Persistencia.

Componentes principales
- `ApiTwitterUala` (Web API)
  - Controllers: `UsersController`, `TweetsController`, `FollowController` — exponen endpoints HTTP y validan DTOs.
  - DTOs: `UserDto`, `TweetDto`, `TweetViewDto` — formas de entrada/salida.
  - `Program.cs`: configuración de DI, EF Core InMemory, caches y background services.

- Dominio
  - Entidades: `User`, `Tweet`, `Follow`.
  - `AppDbContext`: EF Core (InMemory en desarrollo/tests).

- Servicios
  - Caché: `IFollowCacheService`, `ITweetCacheService`, `ITweetCacheUpdaterService` con implementaciones en memoria para desarrollo.
  - Tareas en background: `IBackgroundTaskQueue` + `QueuedHostedService` para actualizaciones asíncronas y trabajo en segundo plano.

- Persistencia
  - EF Core InMemory provider (fácil para pruebas y desarrollo). En producción se reemplazaría por SQL/Redis.

- Tests
  - `ApiTwitterUala.Tests` usa una base en memoria y helpers (e.g., `TestDbContextFactory`, `ModelValidator`) para pruebas unitarias de controllers.

Despliegue / Contenerización
- `ApiTwitterUala/Dockerfile` — multi-stage (build SDK -> publish -> runtime).
- `docker-compose.yml` — servicio `api` mapeando `5000:80`.
- `DOTNET_RUNNING_IN_CONTAINER=true` y `ASPNETCORE_URLS=http://+:80` se usan para evitar redirecciones HTTPS dentro del contenedor.

Flujo de petición (alto nivel)
1. Cliente -> Kestrel (API).  
2. Controller valida DTO y llama a servicios / DbContext.  
3. Escrituras/lecturas en `AppDbContext`. Para caches, se encola trabajo en `IBackgroundTaskQueue`.  
4. `QueuedHostedService` procesa la cola y actualiza caches de forma asíncrona.  
5. Controller retorna DTO / resultado HTTP.

Puntos a revisar / gotchas
- `app.UseHttpsRedirection()` provoca redirección dentro de contenedores sin TLS — usar la variable de entorno o deshabilitarla en container runs.
- Docker build context: construir desde la raíz del repo para incluir proyectos referenciados (`ApiTwitterUala.Domain`, `ApiTwitterUala.Services`) o ajustar `Dockerfile` para el contexto usado.
- Prestar atención a advertencias de nullability en controllers/tests al castear `OkObjectResult.Value`.

Dónde mirar en el repo
- Entrada & DI: `ApiTwitterUala/Program.cs`  
- Controllers: `ApiTwitterUala/Controllers/`  
- Entidades y DbContext: `ApiTwitterUala.Domain/`  
- Servicios y caches: `ApiTwitterUala.Services/` y `ApiTwitterUala/Cache`  
- Tests: `ApiTwitterUala.Tests/`