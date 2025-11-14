FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM node:20-alpine as build-deps
WORKDIR /src
COPY package.json package-lock.json ./
RUN npm i -g gulp-cli
RUN npm i
COPY . ./
RUN gulp

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY --from=build-deps /src/BoothDotDev/wwwroot /src/BoothDotDev/wwwroot
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
