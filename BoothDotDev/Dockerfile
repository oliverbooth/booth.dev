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
COPY --from=build-deps /src/BoothDotDev/wwwroot /src/BoothDotDev/wwwroot
COPY --from=build-deps /src/BoothDotDev/wwwroot/.vite/manifest.json /src/BoothDotDev/wwwroot/.vite/manifest.json
WORKDIR /src
COPY ["BoothDotDev/BoothDotDev.csproj", "BoothDotDev/"]
COPY . .

WORKDIR "/src/BoothDotDev"

FROM build AS publish
RUN dotnet publish "BoothDotDev.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoothDotDev.dll"]