# syntax=docker/dockerfile:1
# Prologue Interpreter — two modes in one image. Batch (default) is a run-to-completion job: it reads the capture
# files the Extractor produced from a mounted /captures folder, interprets them into an event model and writes the
# extraction result and generated Screenplay to the mounted /output folder. Service (--serve / PROLOGUE_MODE=service)
# hosts resumable interpreter sessions over HTTP for Studio: captures come from the Prologue MongoDB database,
# session state persists there too, and the container exits cleanly when a session awaits answers beyond the grace
# period or the service goes idle — the orchestrator restarts it later to resume. Configuration comes from a
# mounted cratis-prologue.json (default /config/cratis-prologue.json).

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
COPY Source/Interpretation/Interpretation.csproj ./Source/Interpretation/
COPY Source/Screenplay/Screenplay.csproj ./Source/Screenplay/
COPY Source/Storage/Storage.csproj ./Source/Storage/
COPY Source/Interpreter/Interpreter.csproj ./Source/Interpreter/

RUN dotnet restore ./Source/Interpreter/Interpreter.csproj -p:Configuration=Release

# Copy source files and publish
COPY README.md ./
COPY Source/Contracts/ ./Source/Contracts/
COPY Source/Configuration/ ./Source/Configuration/
COPY Source/Interpreter.Contracts/ ./Source/Interpreter.Contracts/
COPY Source/Interpretation/ ./Source/Interpretation/
COPY Source/Screenplay/ ./Source/Screenplay/
COPY Source/Storage/ ./Source/Storage/
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=builder /app/out ./

# Batch inputs and outputs are mounted: capture files in /captures, the extraction result and Screenplay land in
# /output, and the optional configuration file at /config/cratis-prologue.json.
ENV PROLOGUE_CONFIG=/config/cratis-prologue.json
ENV PROLOGUE_CAPTURES=/captures
ENV PROLOGUE_OUTPUT=/output/extraction-result.json

# Service mode: PROLOGUE_MODE=service (or --serve), with the HTTP API on PROLOGUE_SERVICE_PORT (default 5004),
# the answer grace period in PROLOGUE_GRACE_PERIOD and the idle timeout in PROLOGUE_IDLE_TIMEOUT (seconds).
EXPOSE 5004

ENTRYPOINT ["dotnet", "Cratis.Prologue.Interpreter.dll"]
