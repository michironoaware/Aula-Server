# https://hub.docker.com/_/microsoft-dotnet
# https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY *.sln .
COPY Aula.Server/*.csproj ./Aula.Server/
RUN dotnet restore

COPY Aula.Server/. ./Aula.Server/
WORKDIR /source/Aula.Server
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./
RUN dotnet ef database update
ENTRYPOINT ["dotnet", "Aula.Server.dll"]