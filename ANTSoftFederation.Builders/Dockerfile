FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Api/ANTSoftFederation.Builders/ Api/ANTSoftFederation.Builders/
COPY Api/ANTSoftFederation.EncryptEFCoreColumn/ Api/ANTSoftFederation.EncryptEFCoreColumn/
COPY Api/ANTSoftFederation.GlobalEnums/ Api/ANTSoftFederation.GlobalEnums/
COPY Api/ANTSoftFederation.KeyVault/ Api/ANTSoftFederation.KeyVault/
COPY Api/ANTSoftFederation.Logger/ Api/ANTSoftFederation.Logger/
COPY Api/ANTSoftFederation.ServiceDefaults/ Api/ANTSoftFederation.ServiceDefaults/
COPY Api/ANTSoftFederation.Shared/ Api/ANTSoftFederation.Shared/
COPY Clients/AntsoftBuilder/src/app/core/builders/page-components.v1.json Clients/AntsoftBuilder/src/app/core/builders/page-components.v1.json

WORKDIR /src/Api/ANTSoftFederation.Builders
RUN dotnet restore SorteFederation.Builders.csproj
RUN dotnet publish SorteFederation.Builders.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "SorteFederation.Builders.dll"]
