FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/WesNews.Api/WesNews.Api.csproj", "src/WesNews.Api/"]
COPY ["src/WesNews.Application/WesNews.Application.csproj", "src/WesNews.Application/"]
COPY ["src/WesNews.Domain/WesNews.Domain.csproj", "src/WesNews.Domain/"]
COPY ["src/WesNews.Infrastructure/WesNews.Infrastructure.csproj", "src/WesNews.Infrastructure/"]
RUN dotnet restore "src/WesNews.Api/WesNews.Api.csproj"
COPY . .
WORKDIR "/src/src/WesNews.Api"
RUN dotnet build "WesNews.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WesNews.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WesNews.Api.dll"]
