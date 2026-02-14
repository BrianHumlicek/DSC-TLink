ARG BUILD_FROM
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_ARCH=amd64
COPY src/ /src/
RUN if [ "$BUILD_ARCH" = "aarch64" ]; then RID="linux-musl-arm64"; else RID="linux-musl-x64"; fi && \
    dotnet publish /src/Demo/Demo.csproj -c Release \
      --self-contained -r $RID \
      -p:PublishSingleFile=true \
      -p:PublishTrimmed=false \
      -o /app

FROM ${BUILD_FROM}
RUN apk add --no-cache libstdc++ libgcc icu-libs
COPY --from=build /app /app
COPY run.sh /

# Register run.sh as an s6-overlay v3 service
RUN chmod a+x /run.sh && \
    mkdir -p /etc/s6-overlay/s6-rc.d/dsc-tlink && \
    echo "longrun" > /etc/s6-overlay/s6-rc.d/dsc-tlink/type && \
    ln -s /run.sh /etc/s6-overlay/s6-rc.d/dsc-tlink/run && \
    mkdir -p /etc/s6-overlay/s6-rc.d/user/contents.d && \
    touch /etc/s6-overlay/s6-rc.d/user/contents.d/dsc-tlink
