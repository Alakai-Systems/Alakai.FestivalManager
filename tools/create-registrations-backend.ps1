# fix_github_workflows.ps1
# Sobreescribe los dos workflows con trigger en master y ambos automaticos
# Ejecutar desde la raiz del repo: .\tools\fix_github_workflows.ps1

$ErrorActionPreference = "Stop"

$dir = ".github\workflows"
if (-not (Test-Path $dir)) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
}

[System.IO.File]::WriteAllText("$dir\deploy-swimout.yml", @'
name: Deploy - Swim Out Costa Brava

on:
  push:
    branches: [ master ]
  workflow_dispatch:

env:
  DOTNET_VERSION: "9.0.x"
  API_PROJECT: "Alakai.FestivalManager.Api/Alakai.FestivalManager.Api.csproj"
  ADMIN_PROJECT: "Alakai.FestivalManager.Admin/Alakai.FestivalManager.Admin.csproj"

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore
        run: dotnet restore Alakai.FestivalManager.sln

      - name: Build
        run: dotnet build Alakai.FestivalManager.sln --configuration Release --no-restore

      - name: Publish API
        run: dotnet publish ${{ env.API_PROJECT }} --configuration Release --output ./publish/api --no-build

      - name: Publish Admin
        run: dotnet publish ${{ env.ADMIN_PROJECT }} --configuration Release --output ./publish/admin --no-build

      - name: Login to Azure
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Deploy API
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_SWIMOUT_API_APP }}
          resource-group-name: ${{ secrets.AZURE_RESOURCE_GROUP_SWIMOUT }}
          package: ./publish/api

      - name: Deploy Admin
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_SWIMOUT_ADMIN_APP }}
          resource-group-name: ${{ secrets.AZURE_RESOURCE_GROUP_SWIMOUT }}
          package: ./publish/admin
'@, [System.Text.Encoding]::UTF8)
Write-Host "OK: deploy-swimout.yml"

[System.IO.File]::WriteAllText("$dir\deploy-lajam.yml", @'
name: Deploy - La Jam Barcelona

on:
  push:
    branches: [ master ]
  workflow_dispatch:

env:
  DOTNET_VERSION: "9.0.x"
  API_PROJECT: "Alakai.FestivalManager.Api/Alakai.FestivalManager.Api.csproj"
  ADMIN_PROJECT: "Alakai.FestivalManager.Admin/Alakai.FestivalManager.Admin.csproj"

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore
        run: dotnet restore Alakai.FestivalManager.sln

      - name: Build
        run: dotnet build Alakai.FestivalManager.sln --configuration Release --no-restore

      - name: Publish API
        run: dotnet publish ${{ env.API_PROJECT }} --configuration Release --output ./publish/api --no-build

      - name: Publish Admin
        run: dotnet publish ${{ env.ADMIN_PROJECT }} --configuration Release --output ./publish/admin --no-build

      - name: Login to Azure
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Deploy API
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_LAJAM_API_APP }}
          resource-group-name: ${{ secrets.AZURE_RESOURCE_GROUP_LAJAM }}
          package: ./publish/api

      - name: Deploy Admin
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_LAJAM_ADMIN_APP }}
          resource-group-name: ${{ secrets.AZURE_RESOURCE_GROUP_LAJAM }}
          package: ./publish/admin
'@, [System.Text.Encoding]::UTF8)
Write-Host "OK: deploy-lajam.yml"

Write-Host ""
Write-Host "Listo. Haz commit y push a master para activar los dos pipelines."