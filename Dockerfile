#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src

COPY Busard.Core/ Busard.Core/
COPY Busard.SqlServer/ Busard.SqlServer/
COPY Busard.Watcher/ Busard.Watcher/

RUN dotnet publish Busard.Watcher -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
USER nobody
ENTRYPOINT ["dotnet", "Busard.Watcher.dll"]
