# syntax=docker/dockerfile:1
# Prologue Receiver — the public-facing service the Prologue Extractor (run in the customer's environment)
# posts correlated captures to when not capturing to file. It persists captures to the shared Prologue MongoDB;
# the Interpreter later analyzes them into an event model.

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
COPY Source/Storage/Storage.csproj ./Source/Storage/
COPY Source/Receiver/Receiver.csproj ./Source/Receiver/

RUN dotnet restore ./Source/Receiver/Receiver.csproj -p:Configuration=Release

# Copy source files and publish
COPY README.md ./
COPY Source/Contracts/ ./Source/Contracts/
COPY Source/Storage/ ./Source/Storage/
COPY Source/Receiver/ ./Source/Receiver/

RUN dotnet publish ./Source/Receiver/Receiver.csproj \
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

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Cratis.Prologue.Receiver.dll"]
