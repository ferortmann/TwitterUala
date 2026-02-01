FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ApiTwitterUala/ApiTwitterUala.csproj ApiTwitterUala/
COPY ApiTwitterUala.Domain/ApiTwitterUala.Domain.csproj ApiTwitterUala.Domain/
COPY ApiTwitterUala.Services/ApiTwitterUala.Services.csproj ApiTwitterUala.Services/

RUN dotnet restore ApiTwitterUala/ApiTwitterUala.csproj

COPY . .

WORKDIR /src/ApiTwitterUala
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "ApiTwitterUala.dll"]