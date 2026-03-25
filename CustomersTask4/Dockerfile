# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CustomersTask4/CustomersTask4.csproj", "CustomersTask4/"]
RUN dotnet restore "./CustomersTask4/CustomersTask4.csproj"
COPY . .
WORKDIR "/src/CustomersTask4"
RUN dotnet build "./CustomersTask4.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CustomersTask4.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtim
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CustomersTask4.dll"]