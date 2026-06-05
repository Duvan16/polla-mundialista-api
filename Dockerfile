FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/PollaMundialista.Api/PollaMundialista.Api.csproj", "PollaMundialista.Api/"]
COPY ["src/PollaMundialista.Application/PollaMundialista.Application.csproj", "PollaMundialista.Application/"]
COPY ["src/PollaMundialista.Domain/PollaMundialista.Domain.csproj", "PollaMundialista.Domain/"]
COPY ["src/PollaMundialista.Infrastructure/PollaMundialista.Infrastructure.csproj", "PollaMundialista.Infrastructure/"]
RUN dotnet restore "PollaMundialista.Api/PollaMundialista.Api.csproj"

COPY src/ .
WORKDIR "/src/PollaMundialista.Api"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PollaMundialista.Api.dll"]
