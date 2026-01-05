# Build with dotnet 9
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

COPY . /app
WORKDIR /app

RUN dotnet restore && dotnet build -c Release

# Runtime
FROM mcr.microsoft.com/dotnet/nightly/aspnet:9.0

RUN mkdir -p /app/bin/wwwroot/cdn

COPY --from=builder /app/bin/Release/net9.0/ /app/bin
COPY ./wwwroot/cdn/ /app/bin/wwwroot/cdn/
WORKDIR /app/bin

EXPOSE 5197

CMD ["/app/bin/CDNServer", "--urls", "http://0.0.0.0:5197"]
