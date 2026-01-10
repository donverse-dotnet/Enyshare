# Build with dotnet 9
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

# Init
RUN apt update && apt install -y wget tar unzip

# Download Assets
WORKDIR /app/assets/libs
RUN wget https://registry.npmjs.org/bootstrap/-/bootstrap-5.3.8.tgz && tar -xvf bootstrap-5.3.8.tgz && \
    mv ./package ./bootstrap && \
    rm -rf bootstrap-5.3.8.tgz

RUN wget https://github.com/twbs/icons/releases/download/v1.13.1/bootstrap-icons-1.13.1.zip && unzip bootstrap-icons-1.13.1.zip && \
    mv ./bootstrap-icons-1.13.1 ./bootstrap-icons && \
    rm -rf bootstrap-icons-1.13.1.zip

RUN wget https://registry.npmjs.org/markdown-it/-/markdown-it-14.1.0.tgz && tar -xvf markdown-it-14.1.0.tgz && \
    mv ./package ./markdown-it-14.1.0/ && \
    rm -rf markdown-it-14.1.0.tgz

WORKDIR /app/assets/images
RUN wget https://placehold.jp/120x120.png && \
    wget https://placehold.jp/3d4070/ffffff/600x350.png

WORKDIR /app/assets/logos
RUN wget https://raw.githubusercontent.com/ggg-alpha/docs/refs/heads/main/src/logo.png && \
    mv logo.png pocco-icon.png

COPY . /app/bin
WORKDIR /app/bin

RUN dotnet restore && dotnet build -c Release

# Runtime
FROM mcr.microsoft.com/dotnet/nightly/aspnet:9.0

RUN mkdir -p /app/bin/wwwroot/cdn

COPY --from=builder /app/bin/bin/Release/net9.0/ /app/bin
COPY --from=builder /app/assets/libs/ /app/bin/wwwroot/cdn/assets
COPY --from=builder /app/assets/images/ /app/bin/wwwroot/cdn/assets/images
COPY --from=builder /app/assets/logos/ /app/bin/wwwroot/cdn/assets/logos
WORKDIR /app/bin

EXPOSE 5197

CMD ["/app/bin/CDNServer", "--urls", "http://0.0.0.0:5197"]
