# run Gulp to build the app
FROM node:12.16.1-alpine as build-stage
COPY package*.json ./
COPY Gulpfile.js ./
RUN npm install --global gulp-cli
RUN npm install
RUN gulp

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OliverBooth/OliverBooth.csproj", "OliverBooth/"]
RUN dotnet restore "OliverBooth/OliverBooth.csproj"
COPY . .
WORKDIR "/src/OliverBooth"
RUN dotnet build "OliverBooth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OliverBooth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OliverBooth.dll"]
