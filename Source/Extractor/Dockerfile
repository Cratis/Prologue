# syntax=docker/dockerfile:1
# Prologue Extractor — runs next to the system being captured. Captures SQL Server CDC, Postgres logical
# replication, HTTP commands (reverse proxy) and OpenTelemetry, and writes capture files or posts to the Receiver.
# Configuration comes from a mounted cratis-prologue.json (default /config/cratis-prologue.json).

ARG VERSION=0.0.0-dev
ARG COMMIT=dev

####################################
# Stage 1: Build
####################################
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

ARG VERSION
ARG COMMIT

WORKDIR /src

# Copy project files and restore
COPY Directory.Build.props Directory.Packages.props global.json .editorconfig .globalconfig ./
COPY Source/Directory.Build.props ./Source/
COPY Source/Contracts/Contracts.csproj ./Source/Contracts/
COPY Source/Configuration/Configuration.csproj ./Source/Configuration/
COPY Source/Extractor/Extractor.csproj ./Source/Extractor/

RUN dotnet restore ./Source/Extractor/Extractor.csproj -p:Configuration=Release

# Copy source files and publish
COPY README.md ./
COPY Source/Contracts/ ./Source/Contracts/
COPY Source/Configuration/ ./Source/Configuration/
COPY Source/Extractor/ ./Source/Extractor/

RUN dotnet publish ./Source/Extractor/Extractor.csproj \
    -c Release \
    -p:Version=${VERSION} \
    -p:SourceRevisionId=${COMMIT} \
    -p:EnableSourceControlManagerQueries=false \
    -p:EnableSourceLink=false \
    -o /app/out

####################################
# Stage 2: Runtime
####################################
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=builder /app/out ./

# Configuration is mounted at /config; captured files land in /captures when file output is configured.
ENV PROLOGUE_CONFIG=/config/cratis-prologue.json

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080 4317 4318

ENTRYPOINT ["dotnet", "Cratis.Prologue.Extractor.dll"]
