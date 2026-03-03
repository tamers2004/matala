FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

COPY Backend.Api/Backend.Api.csproj Backend.Api/
RUN dotnet restore Backend.Api/Backend.Api.csproj

CMD ["dotnet", "watch", "run", "--project", "Backend.Api", "--no-launch-profile"]
