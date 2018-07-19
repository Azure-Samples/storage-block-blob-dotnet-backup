FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY backup.utility/backup.utility/backup.utility.csproj backup.utility/backup.utility/
COPY backup.core/backup.core.csproj backup.core/
RUN dotnet restore backup.utility/backup.utility/backup.utility.csproj
COPY . .
WORKDIR /src/backup.utility/backup.utility
RUN dotnet build backup.utility.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish backup.utility.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "backup.utility.dll"]
