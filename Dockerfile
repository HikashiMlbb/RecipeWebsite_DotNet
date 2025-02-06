FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c release -o ./build --restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app/
COPY --from=build app/build/* ./
ENTRYPOINT [ "dotnet", "API.dll" ]