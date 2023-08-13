FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OliverBooth.Blog/OliverBooth.Blog.csproj", "OliverBooth.Blog/"]
RUN dotnet restore "OliverBooth.Blog/OliverBooth.Blog.csproj"
COPY . .
WORKDIR "/src/OliverBooth.Blog"
RUN dotnet build "OliverBooth.Blog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OliverBooth.Blog.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OliverBooth.Blog.dll"]
