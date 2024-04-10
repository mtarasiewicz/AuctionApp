FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /app
EXPOSE 80

# copy all .csproj files 

COPY AuctionApp.sln AuctionApp.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj

# restore package deps

RUN dotnet restore AuctionApp.sln

#copy the app folder

COPY src/GatewayService src/GatewayService
WORKDIR /app/src/GatewayService
RUN dotnet publish -c Release -o /app/src/out

# build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/src/out . 
ENTRYPOINT [ "dotnet", "GatewayService.dll" ]