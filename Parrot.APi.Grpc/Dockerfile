FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Parrot.APi.Grpc/Parrot.APi.Grpc.csproj", "Parrot.APi.Grpc/"]
COPY ["Parrot.Application/Parrot.Application.csproj", "Parrot.Application/"]
COPY ["Parrot.Domain/Parrot.Domain.csproj", "Parrot.Domain/"]
COPY ["Parrot.Infrastructure/Parrot.Infrastructure.csproj", "Parrot.Infrastructure/"]
COPY ["Parrot.Grpc.Contracts/Parrot.Grpc.Contracts.csproj", "Parrot.Grpc.Contracts/"]
RUN dotnet restore "Parrot.APi.Grpc/Parrot.APi.Grpc.csproj"

COPY . .
WORKDIR "/src/Parrot.APi.Grpc"
RUN dotnet publish "Parrot.APi.Grpc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Parrot.APi.Grpc.dll"]
