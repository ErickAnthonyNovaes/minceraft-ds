# Use Microsoft's official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY *.csproj ./
RUN dotnet restore --verbosity minimal

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# Final runtime image: ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Allow the runtime to bind to the port provided by the host (Render sets $PORT)
# If $PORT is not set, fallback to 80
ENV PORT=${PORT:-80}
ENV ASPNETCORE_URLS=http://+:${PORT}

# Copy published output from build stage
COPY --from=build /app/publish ./

# Expose the default container port
EXPOSE 80

# Run the app
ENTRYPOINT ["dotnet", "minceraftapi.dll"]
