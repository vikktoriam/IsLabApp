# ЭТАП 1: Build (Сборка)
# Используем официальный образ SDK для компиляции
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости (отдельный слой для кэширования)
COPY ["IsLabApp.csproj", "./"]
RUN dotnet restore

# Копируем остальные исходники и собираем приложение
COPY . .
RUN dotnet publish "IsLabApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ЭТАП 2: Runtime (Запуск)
# Используем легкий официальный runtime-образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Копируем только результат сборки из предыдущего этапа
COPY --from=build /app/publish .

# Выставляем порт приложения (внутри контейнера)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Точка входа для запуска приложения
ENTRYPOINT ["dotnet", "IsLabApp.dll"]
