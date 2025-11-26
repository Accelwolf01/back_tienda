FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["back_tienda.csproj", "./"]
RUN dotnet restore "back_tienda.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "back_tienda.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "back_tienda.dll"]
