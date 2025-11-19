# 🧬 TestZombis-API

<div align="center">

### API for Genetic Sequence Detection (Zombie, Covid, Influenza, etc.)

![DotNet](https://img.shields.io/badge/.NET-8.0-blueviolet?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-Backend-green?style=for-the-badge&logo=csharp)
![Architecture](https://img.shields.io/badge/Architecture-CleanArchitecture-orange?style=for-the-badge)
![Patterns](https://img.shields.io/badge/Patterns-Factory%20%2B%20Strategy-blue?style=for-the-badge)
![Database](https://img.shields.io/badge/Database-SQL%20Server-red?style=for-the-badge&logo=Microsoft%20SQL%20Server)
![Status](https://img.shields.io/badge/Status-Active-brightgreen?style=for-the-badge)

</div>

## 🧠 Description

**TestZombis-API** is a backend system built in **.NET 8** designed to analyze NxN genetic matrices and determine infection types such as **Zombie**, **Covid**, **Influenza**, and more.

The API implements:

- **Factory Pattern**
- **Strategy Pattern**
- **Clean Architecture**

> ❗ This API does **NOT** use EF migrations. Database must be built using SQL script.

## 🏗 Architecture

```
┌───────────────────────┐
│       Web API          │
└─────────────┬─────────┘
              ▼
┌───────────────────────┐
│      Application       │
└─────────────┬─────────┘
              ▼
┌───────────────────────┐
│       Domain           │
└─────────────┬─────────┘
              ▼
┌───────────────────────┐
│    Infrastructure      │
└───────────────────────┘
```

## 🔁 Sequence Diagram

```mermaid
sequenceDiagram
Client->>Controller: POST /diagnosis/{type}
Controller->>DiagnosisService: CreateDiagnosisAsync
DiagnosisService->>DiagnosisFactory: GetStrategy
DiagnosisFactory->>Strategy: Resolve
Strategy-->>DiagnosisService: Result
DiagnosisService->>DbContext: Save
DbContext-->>DiagnosisService: OK
DiagnosisService-->>Controller: Response
Controller-->>Client: 200 OK
```

## 🧩 Strategy Diagram

```mermaid
classDiagram
IDiagnosisStrategy <|.. ZombieDiagnosisStrategy
IDiagnosisStrategy <|.. CovidDiagnosisStrategy
IDiagnosisStrategy <|.. InfluenzaDiagnosisStrategy
```

## 🚀 Run

```
git clone https://github.com/luisgabrielahumada/TestZombis-Api.git
dotnet run
```

## 📡 Endpoint

```
POST /api/diagnosis/{diagnosisType}
```

### Example

```json
{
  "patientFullName": "John Connor",
  "patientDocumentNumber": "DOC55555",
  "patientGender": "Male",
  "diagnosisType": "zombie",
  "geneticCode": [
    "PLAGGP",
    "APGLGP",
    "LLALGL",
    "APLAPL",
    "PPPPLA",
    "LAPLGG"
  ]
}
```

