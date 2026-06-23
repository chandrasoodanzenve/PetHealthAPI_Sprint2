FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PetHealthAPI.csproj", "."]
RUN dotnet restore "./PetHealthAPI.csproj"
COPY . .
RUN dotnet build "PetHealthAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PetHealthAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PetHealthAPI.dll"]