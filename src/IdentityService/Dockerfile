FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Carsties.sln ./
COPY src/IdentityService/IdentityService.csproj ./src/IdentityService/
COPY src/BiddingService/BiddingService.csproj ./src/BiddingService/
COPY tests/AuctionService.UnitTests/AuctionService.UnitTests.csproj ./tests/AuctionService.UnitTests/
COPY tests/AuctionService.IntegrationTests/AuctionService.IntegrationTests.csproj ./tests/AuctionService.IntegrationTests/

RUN dotnet restore ./src/IdentityService/IdentityService.csproj

COPY . .
RUN dotnet publish ./src/IdentityService/IdentityService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "IdentityService.dll" ]