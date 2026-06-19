# syntax=docker/dockerfile:1.7

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY GwmOra.sln ./
COPY src/LibGwmApi/LibGwmApi.csproj src/LibGwmApi/
COPY src/GwmOra.Addon/GwmOra.Addon.csproj src/GwmOra.Addon/
RUN dotnet restore src/GwmOra.Addon/GwmOra.Addon.csproj

COPY src/LibGwmApi/ src/LibGwmApi/
COPY src/GwmOra.Addon/ src/GwmOra.Addon/
RUN dotnet publish src/GwmOra.Addon/GwmOra.Addon.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
ARG BUILD_VERSION=dev
ARG BUILD_ARCH=amd64
WORKDIR /app

LABEL \
    io.hass.version=${BUILD_VERSION} \
    io.hass.type="app" \
    io.hass.arch=${BUILD_ARCH}

COPY openssl.cnf /app/openssl.cnf
COPY src/LibGwmApi/Resources/gwm_root.pem /etc/ssl/certs/gwm_root.pem
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://0.0.0.0:8099
ENV OPENSSL_CONF=/app/openssl.cnf

EXPOSE 8099
ENTRYPOINT ["dotnet", "GwmOra.Addon.dll"]
