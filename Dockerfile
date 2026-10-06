FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Application.Api/Application.Api.csproj", "Application.Api/"]
COPY ["Application.Core/Application.Core.csproj", "Application.Core/"]
COPY ["Application.Repository/Application.Repositories.csproj", "Application.Repository/"]
COPY ["Application.Services/Application.Services.csproj", "Application.Services/"]
COPY ["Application.RequestModel/Application.RequestModel.csproj", "Application.RequestModel/"]
COPY ["Application.ViewModel/Application.ViewModel.csproj", "Application.ViewModel/"]

RUN dotnet restore "Application.Api/Application.Api.csproj"

COPY . .
WORKDIR "/src/Application.Api"
RUN dotnet build "Application.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Application.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Application.Api.dll"]
