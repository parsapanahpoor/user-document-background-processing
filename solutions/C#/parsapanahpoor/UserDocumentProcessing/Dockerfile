FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/UserDocumentAPI/UserDocumentAPI.csproj", "src/UserDocumentAPI/"]
RUN dotnet restore "src/UserDocumentAPI/UserDocumentAPI.csproj"
COPY . .
WORKDIR "/src/src/UserDocumentAPI"
RUN dotnet build "UserDocumentAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserDocumentAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserDocumentAPI.dll"]
