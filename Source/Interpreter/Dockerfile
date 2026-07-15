# syntax=docker/dockerfile:1
# Prologue Interpreter — a run-to-completion job. It reads the capture files the Extractor produced from a mounted
# /captures folder, interprets them into an event model and writes the extraction result to the mounted /output
# folder. Configuration comes from a mounted cratis-prologue.json (default /config/cratis-prologue.json).

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
COPY Source/Interpreter.Contracts/Interpreter.Contracts.csproj ./Source/Interpreter.Contracts/
COPY Source/Interpreter/Interpreter.csproj ./Source/Interpreter/

RUN dotnet restore ./Source/Interpreter/Interpreter.csproj -p:Configuration=Release

# Copy source files and publish
COPY README.md ./
COPY Source/Contracts/ ./Source/Contracts/
COPY Source/Configuration/ ./Source/Configuration/
COPY Source/Interpreter.Contracts/ ./Source/Interpreter.Contracts/
COPY Source/Interpreter/ ./Source/Interpreter/

RUN dotnet publish ./Source/Interpreter/Interpreter.csproj \
    -c Release \
    -p:Version=${VERSION} \
    -p:SourceRevisionId=${COMMIT} \
    -p:EnableSourceControlManagerQueries=false \
    -p:EnableSourceLink=false \
    -o /app/out

####################################
# Stage 2: Runtime
####################################
FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app
COPY --from=builder /app/out ./

# Inputs and outputs are mounted: capture files in /captures, the extraction result lands in /output, and the
# optional configuration file at /config/cratis-prologue.json.
ENV PROLOGUE_CONFIG=/config/cratis-prologue.json
ENV PROLOGUE_CAPTURES=/captures
ENV PROLOGUE_OUTPUT=/output/extraction-result.json

ENTRYPOINT ["dotnet", "Cratis.Prologue.Interpreter.dll"]
