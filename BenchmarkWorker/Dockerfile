FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/BenchmarkWorker"
ENTRYPOINT ["dotnet","run"," --project", "BenchmarkWorker.csproj", "-c","Release"]