# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

COPY ./src/ /app
COPY NuGet.config /app
WORKDIR /app/server/
RUN dotnet restore && dotnet build -c Release

# Runtime stage
FROM mcr.microsoft.com/dotnet/nightly/aspnet:9.0

COPY --from=builder /app/server/bin/Release/net9.0/ /app/bin

EXPOSE 5290

CMD ["/app/bin/Pocco.Srv.Auth"]
