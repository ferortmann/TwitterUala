# TwitterUala (ApiTwitterUala)

Crear una versión simplificada de una plataforma de microblogging similar a twitter

## Requerimientos
- SDK de .NET 8 (instalar desde https://dotnet.microsoft.com)
- (Opcional) Visual Studio 2022/2023 con la carga de trabajo de .NET 8
- (Opcional) Herramientas de EF Core: `dotnet tool install --global dotnet-ef` (solo si necesitas ejecutar migraciones)

## Proyectos
- `ApiTwitterUala` — Proyecto de Web API
- `ApiTwitterUala.Tests` — Proyecto de pruebas unitarias
- Archivo de solución: `TwitterUala.sln` (abrir con Visual Studio o `dotnet`)

## Configuración (CLI)
1. Clona el repositorio:
   - `git clone <repo-url>`
2. Restaura y compila:
   - `dotnet restore`
   - `dotnet build`
3. Si la aplicación utiliza una base de datos, verifica `appsettings.json` para obtener la cadena de conexión y ejecuta las migraciones (si están presentes):
   - `dotnet ef database update --project ApiTwitterUala --startup-project ApiTwitterUala`

## Ejecutar la API
- Desde la carpeta del proyecto:
  - `dotnet run --project ApiTwitterUala`
- La API se iniciará en la URL configurada de Kestrel (ver la salida de la consola).

## Ejecutar pruebas
- CLI:
  - `dotnet test`
- Visual Studio:
  - Abre `TwitterUala.sln`
  - Ejecuta las pruebas desde __Test Explorer__
  - Verifica la salida de las pruebas en el panel de __Output__ (seleccionar `Tests`)

## Pasos rápidos en Visual Studio
1. Abre `TwitterUala.sln`.
2. Establece `ApiTwitterUala` como proyecto de inicio.
3. Usa __Debug__ > __Start Debugging__ (F5) o __Start Without Debugging__ (Ctrl+F5).
4. Usa __Test Explorer__ para ejecutar las pruebas unitarias.

## Solución de problemas comunes
- "SDK no encontrado" — instala el SDK de .NET 8 y reinicia el IDE/terminal.
- Conflicto de puertos — cambia la URL de la aplicación en `launchSettings.json` o detén el proceso que ocupa el puerto.
- Migraciones faltantes/errores al conectar DB — verifica la cadena de conexión en `appsettings.Development.json` y ejecuta las migraciones si es necesario.

## Docker

Estas instrucciones construyen y ejecutan la API en Docker (usa el `Dockerfile` en `ApiTwitterUala/Dockerfile`).

- Build de la imagen (desde la raíz del repositorio):
  - `docker build -t apitwitteruala:latest -f ApiTwitterUala/Dockerfile .`

- Run del contenedor (HTTP host:5000 -> contenedor:80):
  - `docker run --rm -it -p 5000:80 -e ASPNETCORE_URLS=http://+:80 -e ASPNETCORE_ENVIRONMENT=Development --name apitwitteruala apitwitteruala:latest`

- Docker Compose (desde la raíz del repositorio):
  - Build y ejecutar en primer plano (muestra logs):
    - `docker compose up --build`
  - Build y ejecutar en segundo plano (detached):
    - `docker compose up -d --build`
  - Parar y limpiar:
    - `docker compose down`

Notas:
- Si `Program.cs` tiene `app.UseHttpsRedirection()`, usa `-e ASPNETCORE_URLS=http://+:80` al correr el contenedor para evitar redirecciones a HTTPS no servidas en el contenedor, o configura certificados/exponer puerto 443.
- Si `docker build` falla por proyectos faltantes, asegúrate de ejecutar el `docker build` desde la raíz del repositorio para que el contexto incluya `ApiTwitterUala.Domain` y `ApiTwitterUala.Services`.

## Notas y próximos pasos
- Las pruebas utilizan una base de datos en memoria (ver `ApiTwitterUala.Tests`) — deberían ejecutarse sin configuración adicional.
- Si deseas un README personalizado (tipo de DB, variables de entorno requeridas, solicitudes de ejemplo), indícame qué base de datos prefieres y cómo deseas ejecutar la aplicación (CLI o Visual Studio).