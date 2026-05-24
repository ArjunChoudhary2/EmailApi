FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY EmailSender.Api/EmailSender.Api.csproj EmailSender.Api/
COPY EmailSender.Application/EmailSender.Application.csproj EmailSender.Application/
COPY EmailSender.Infrastructure/EmailSender.Infrastructure.csproj EmailSender.Infrastructure/
COPY EmailSender.Domain/EmailSender.Domain.csproj EmailSender.Domain/

RUN dotnet restore EmailSender.Api/EmailSender.Api.csproj

# Copy everything else and publish
COPY . .
WORKDIR /src/EmailSender.Api
RUN dotnet publish EmailSender.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render sets PORT environment variable; expose and configure default
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EmailSender.Api.dll"]
