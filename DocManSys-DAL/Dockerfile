# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8082


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["DocManSys-DAL/DocManSys-DAL.csproj", "DocManSys-DAL/"]
RUN dotnet restore "./DocManSys-DAL/DocManSys-DAL.csproj"

COPY . .

RUN dotnet build "./DocManSys-DAL/DocManSys-DAL.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
RUN dotnet publish "./DocManSys-DAL/DocManSys-DAL.csproj" -c Release -o /app/publish 

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocManSys-DAL.dll"]