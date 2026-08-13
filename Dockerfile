FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.slnx ./
COPY GreenBasket.API/*.csproj ./GreenBasket.API/
COPY GreenBasket.Application/*.csproj ./GreenBasket.Application/
COPY GreenBasket.Domain/*.csproj ./GreenBasket.Domain/
COPY GreenBasket.Infrastructure/*.csproj ./GreenBasket.Infrastructure/
COPY GreenBasket.API.Tests/*.csproj ./GreenBasket.API.Tests/
COPY GreenBasket.Application.Tests/*.csproj ./GreenBasket.Application.Tests/

# Restore dependencies for the API project
RUN dotnet restore GreenBasket.API/GreenBasket.API.csproj

# Copy the remaining source code
COPY . .

# Build and publish the API
WORKDIR /app/GreenBasket.API
RUN dotnet publish -c Release -o /out

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

# Ensure uploads directory exists
RUN mkdir -p /app/wwwroot/uploads

ENTRYPOINT ["dotnet", "GreenBasket.API.dll"]
