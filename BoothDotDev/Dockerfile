FROM mcr.microsoft.com/dotnet/aspnet:11.0-preview AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM node:20-alpine as build-deps
WORKDIR /src
COPY package.json package-lock.json ./
RUN npm ci
COPY . ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:11.0-preview AS build
# scripts/source-revision.sh asks the git server for the commit when the build has no .git of its own
RUN apt-get update && apt-get install -y --no-install-recommends git ca-certificates && rm -rf /var/lib/apt/lists/*
COPY --from=build-deps /src/BoothDotDev/wwwroot /src/BoothDotDev/wwwroot
COPY --from=build-deps /src/BoothDotDev/wwwroot/.vite/manifest.json /src/BoothDotDev/wwwroot/.vite/manifest.json
WORKDIR /src
COPY ["BoothDotDev/BoothDotDev.csproj", "BoothDotDev/"]
COPY . .

WORKDIR "/src/BoothDotDev"

FROM build AS publish
ARG SOURCE_REVISION_ID=
ARG GIT_REF=
ARG GIT_REPOSITORY=https://git.booth.dev/oliver/booth.dev.git
RUN rev=$(SOURCE_REVISION_ID="$SOURCE_REVISION_ID" GIT_REF="$GIT_REF" GIT_REPOSITORY="$GIT_REPOSITORY" \
        sh /src/scripts/source-revision.sh /src) && \
    dotnet publish "BoothDotDev.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:SourceRevisionId="$rev"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoothDotDev.dll"]
