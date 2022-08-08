FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["api/FinaSwap.Api/FinaSwap.Api.csproj", "FinaSwap.Api/"]
RUN dotnet restore "FinaSwap.Api/FinaSwap.Api.csproj"
COPY . .
WORKDIR "/src/FinaSwap.Api"
RUN dotnet build "FinaSwap.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinaSwap.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinaSwap.Api.dll"]