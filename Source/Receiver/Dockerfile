# syntax=docker/dockerfile:1
# Prologue Receiver — also hosts the Workbench (Arc + Chronicle + the React frontend). It is the public-facing service the Prologue Extractor (run in the customer's environment)
# posts correlated captures to when not capturing to file. It persists captures to the shared Prologue MongoDB;
# the Interpreter later analyzes them into an event model.

ARG VERSION=0.0.0-dev
ARG COMMIT=dev

####################################
# Stage 1: Workbench frontend
####################################
# Builds the React frontend into Source/Receiver/wwwroot (the Vite outDir), where the Receiver serves it from.
FROM --platform=$BUILDPLATFORM node:22-slim AS frontend

WORKDIR /src

RUN corepack enable

COPY package.json yarn.lock .yarnrc.yml ./
COPY Source/Workbench/package.json ./Source/Workbench/
RUN yarn install --immutable

COPY Source/Workbench/ ./Source/Workbench/
RUN yarn build

####################################
# Stage 2: Build
####################################
# The build stage runs on the builder's own platform and publishes portable, framework-dependent output, so
# one build serves every target platform without emulation. Only the runtime stage below is per-platform.
# The runtime stage below installs git for cloning system repositories, so it has a RUN step and the arm64 build
# needs docker/setup-qemu-action (publish.yml has it).
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0.100-rc.3-resolute AS builder

ARG VERSION
ARG COMMIT

WORKDIR /src

# Copy project files and restore
COPY Directory.Build.props Directory.Packages.props global.json .editorconfig .globalconfig ./
COPY Source/Directory.Build.props Source/Directory.Build.targets ./Source/
COPY Source/Configuration/Configuration.csproj ./Source/Configuration/
COPY Source/Contracts/Contracts.csproj ./Source/Contracts/
COPY Source/Interpreter.Contracts/Interpreter.Contracts.csproj ./Source/Interpreter.Contracts/
COPY Source/Interpretation/Interpretation.csproj ./Source/Interpretation/
COPY Source/Screenplay/Screenplay.csproj ./Source/Screenplay/
COPY Source/Storage/Storage.csproj ./Source/Storage/
COPY Source/Workbench/Workbench.csproj ./Source/Workbench/
COPY Source/Receiver/Receiver.csproj ./Source/Receiver/

RUN dotnet restore ./Source/Receiver/Receiver.csproj -p:Configuration=Release

# Copy source files and publish
COPY README.md ./
COPY Source/Configuration/ ./Source/Configuration/
COPY Source/Contracts/ ./Source/Contracts/
COPY Source/Interpreter.Contracts/ ./Source/Interpreter.Contracts/
COPY Source/Interpretation/ ./Source/Interpretation/
COPY Source/Screenplay/ ./Source/Screenplay/
COPY Source/Storage/ ./Source/Storage/
COPY Source/Workbench/ ./Source/Workbench/
COPY Source/Receiver/ ./Source/Receiver/
COPY --from=frontend /src/Source/Receiver/wwwroot ./Source/Receiver/wwwroot

RUN dotnet publish ./Source/Receiver/Receiver.csproj \
    -c Release \
    -p:Version=${VERSION} \
    -p:SourceRevisionId=${COMMIT} \
    -p:EnableSourceControlManagerQueries=false \
    -p:EnableSourceLink=false \
    -o /app/out

####################################
# Stage 3: Runtime
####################################
FROM mcr.microsoft.com/dotnet/aspnet:11.0.0-rc.3-resolute

# The Workbench clones the repositories systems live in with the git command line.
RUN apt-get update \
    && apt-get install -y --no-install-recommends git \
    && rm -rf /var/lib/apt/lists/*

# Clones live under /var/prologue/repos (the default Prologue:Repositories:CloneDirectory); the image runs as the
# non-root aspnet user, so that directory must belong to it. Mount a volume there to keep clones across restarts.
RUN mkdir -p /var/prologue/repos \
    && chown -R $APP_UID /var/prologue

WORKDIR /app
COPY --from=builder /app/out ./

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "Cratis.Prologue.Receiver.dll"]
