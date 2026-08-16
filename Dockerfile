FROM gcc:13-bookworm AS cpp-build
RUN apt-get update && apt-get install -y --no-install-recommends \
    cmake build-essential git python3 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /cpp
COPY algorithms/ .
RUN cmake -B build -S . -DCMAKE_BUILD_TYPE=Release -DSW_BUILD_TESTS=OFF
RUN cmake --build build -j$(nproc)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY core/ .
RUN dotnet publish src/Sidwell.Core/Sidwell.Core.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
COPY --from=cpp-build /cpp/build/libsidwell_core_algorithms.so .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Sidwell.Core.dll"]
