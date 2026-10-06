FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TicketApp.csproj", "./"]
RUN dotnet restore "TicketApp.csproj"

COPY . .
RUN dotnet publish "TicketApp.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5040
ENV ASPNETCORE_URLS=http://+:5040

ENTRYPOINT ["dotnet", "TicketApp.dll"]