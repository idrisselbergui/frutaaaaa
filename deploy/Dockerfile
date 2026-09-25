# Runtime-only image. The API and the web app are built on the developer PC by
# build-package.ps1 and copied in ready-made (Docker Hub is not reachable from the
# server; this base image comes from mcr.microsoft.com).
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY app/ ./

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "frutaaaaa.dll"]
