FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["WordCloudService.csproj", "./"]
RUN dotnet restore "WordCloudService.csproj"
COPY . .
RUN dotnet publish "WordCloudService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 15003
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WordCloudService.dll"]