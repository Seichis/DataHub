FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["DataHub/DataHub.csproj", "DataHub/"]
COPY ["DataHub.Entities/DataHub.Entities.csproj", "DataHub.Entities/"]
RUN dotnet restore "DataHub/DataHub.csproj"
COPY . .
WORKDIR "/src/DataHub"
RUN dotnet build "DataHub.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DataHub.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DataHub.dll"]