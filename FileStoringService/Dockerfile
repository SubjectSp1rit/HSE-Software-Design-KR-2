FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Common/Common.csproj", "Common/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["FileStoringService/FileStoringService.csproj", "FileStoringService/"]

RUN dotnet restore "FileStoringService/FileStoringService.csproj"

COPY . .
WORKDIR "/src/FileStoringService"
RUN dotnet publish "FileStoringService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 15001
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FileStoringService.dll"]
