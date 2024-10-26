FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY *.sln ./
COPY src/SharpQueue.NotificationService/*.csproj ./src/SharpQueue.NotificationService/
COPY src/SharpQueue.Core/*.csproj ./src/SharpQueue.Core/
COPY src/SharpQueue.Http/*.csproj ./src/SharpQueue.Http/
COPY tests/SharpQueue.Tests/*.csproj ./tests/SharpQueue.Tests/
RUN dotnet restore

COPY src/SharpQueue.NotificationService/ ./src/SharpQueue.NotificationService/
COPY src/SharpQueue.Core/ ./src/SharpQueue.Core/
RUN dotnet publish src/SharpQueue.NotificationService -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS=http://0.0.0.0:5151
EXPOSE 5151

ENTRYPOINT ["dotnet", "SharpQueue.NotificationService.dll"]