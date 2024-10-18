FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DocManSys-DAL/DocManSys-DAL.csproj", "DocManSys-DAL/"]
RUN dotnet restore "./DocManSys-DAL/DocManSys-DAL.csproj"
COPY . .
WORKDIR "/src/DocManSys-DAL"
RUN dotnet build "./DocManSys-DAL.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DocManSys-DAL.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocManSys-DAL.dll"]
