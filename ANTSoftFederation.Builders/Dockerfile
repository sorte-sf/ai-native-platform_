FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Api/SorteFederation.Builders/ ./Api/SorteFederation.Builders/
COPY Api/SorteFederation.EncryptEFCoreColumn/ ./Api/SorteFederation.EncryptEFCoreColumn/
COPY Api/SorteFederation.GlobalEnums/ ./Api/SorteFederation.GlobalEnums/
COPY Api/SorteFederation.KeyVault/ ./Api/SorteFederation.KeyVault/
COPY Api/SorteFederation.Logger/ ./Api/SorteFederation.Logger/
COPY Api/SorteFederation.ServiceDefaults/ ./Api/SorteFederation.ServiceDefaults/
COPY Api/SorteFederation.Shared/ ./Api/SorteFederation.Shared/
WORKDIR /src/Api/SorteFederation.Builders
RUN dotnet restore SorteFederation.Builders.csproj
RUN dotnet publish SorteFederation.Builders.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "SorteFederation.Builders.dll"]
