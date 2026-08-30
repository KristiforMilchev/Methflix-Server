#syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MethflixServer.sln .
COPY API/API.csproj API/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY CLI/CLI.csproj CLI/
RUN dotnet restore API/API.csproj

COPY . .
RUN dotnet publish API/API.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Headless Chromium's runtime shared libs for the indexer's headless-browser fetch
# strategy (gets past Cloudflare's JS challenge on sites like 1337x, which a plain
# HTTP GET can't do). Ubuntu's own "chromium" apt package is a Snap-redirect stub
# that doesn't work in containers, so PuppeteerSharp downloads a real, self-contained
# Chromium build on first use (HeadlessBrowserPageFetcher) - these libs are what that
# build needs to actually launch.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates fonts-liberation libasound2t64 libatk-bridge2.0-0 libatk1.0-0 \
        libcairo2 libcups2t64 libdbus-1-3 libdrm2 libexpat1 libgbm1 libglib2.0-0t64 \
        libgtk-3-0t64 libnspr4 libnss3 libpango-1.0-0 libpangocairo-1.0-0 libx11-6 \
        libx11-xcb1 libxcb1 libxcomposite1 libxdamage1 libxext6 libxfixes3 \
        libxkbcommon0 libxrandr2 libxshmfence1 wget \
    && rm -rf /var/lib/apt/lists/*

COPY --from=mwader/static-ffmpeg:7.0.2 /ffmpeg /usr/local/bin/ffmpeg
COPY --from=mwader/static-ffmpeg:7.0.2 /ffprobe /usr/local/bin/ffprobe

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000 \
    ASPNETCORE_ENVIRONMENT=Production \
    FfMPEG=/usr/local/bin

EXPOSE 5000

ENTRYPOINT ["dotnet", "API.dll"]
