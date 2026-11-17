FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ANTSoftFederation.Builders/ ANTSoftFederation.Builders/
COPY ANTSoftFederation.EncryptEFCoreColumn/ ANTSoftFederation.EncryptEFCoreColumn/
COPY ANTSoftFederation.GlobalEnums/ ANTSoftFederation.GlobalEnums/
COPY ANTSoftFederation.KeyVault/ ANTSoftFederation.KeyVault/
COPY ANTSoftFederation.Logger/ ANTSoftFederation.Logger/
COPY ANTSoftFederation.ServiceDefaults/ ANTSoftFederation.ServiceDefaults/
COPY ANTSoftFederation.Shared/ ANTSoftFederation.Shared/

WORKDIR /src/ANTSoftFederation.Builders
RUN dotnet restore SorteFederation.Builders.csproj
RUN dotnet publish SorteFederation.Builders.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "SorteFederation.Builders.dll"]
