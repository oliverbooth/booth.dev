FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OliverBooth/OliverBooth.csproj", "OliverBooth/"]
RUN dotnet restore "OliverBooth/OliverBooth.csproj"
COPY . .

RUN apt-get update && apt-get install -y curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash -
RUN apt-get install -y nodejs
RUN npm i -g gulp-cli
RUN npm i
RUN gulp

WORKDIR "/src/OliverBooth"
RUN dotnet build "OliverBooth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OliverBooth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OliverBooth.dll"]
