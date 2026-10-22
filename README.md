## Kompilacja

```powershell
dotnet build .\src\MedConnect.csproj
```

## Uruchomienie bazy danych:

```powershell
docker compose up -d
```


## Migracja:

```powershell
dotnet ef database update --project .\src\MedConnect.csproj
```

## Uruchomienie aplikacji:

```powershell
dotnet run --project .\src\MedConnect.csproj 
```