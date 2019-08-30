FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["RabbitMQAzureMetricsRunner/RabbitMQAzureMetricsRunner.csproj", "RabbitMQAzureMetricsRunner/"]
COPY ["RabbitMQAzureMetrics/RabbitMQAzureMetrics.csproj", "RabbitMQAzureMetrics/"]
RUN dotnet restore "RabbitMQAzureMetricsRunner/RabbitMQAzureMetricsRunner.csproj"
COPY . .
WORKDIR "/src/RabbitMQAzureMetricsRunner"
RUN dotnet build "RabbitMQAzureMetricsRunner.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RabbitMQAzureMetricsRunner.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RabbitMQAzureMetricsRunner.dll"]