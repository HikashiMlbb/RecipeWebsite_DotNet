FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c release -o ./build --restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app/
RUN apt-get update && apt-get install -y curl
COPY --from=build app/build/* ./
EXPOSE 8080
ENTRYPOINT [ "dotnet", "API.dll" ]
