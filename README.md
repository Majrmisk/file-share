# FileShare

A simple web app for sharing files, built with ASP.NET Core Razor Pages.

## Features
- Email sign-up / login
- Upload files to **Azure Blob Storage**
- **Create permanent links** (one per file)
- Public **download pages**
- Quotas: ~10 accounts total, **50 MB per user** (admin no cap)

## Live Website
This app is deployed on Microsoft Azure: [FileShare](https://filesharemajrmisk-bfdhd9fueaekerh2.westeurope-01.azurewebsites.net/)

## Technologies Used
| Purpose            | Package / Service            |
|--------------------|------------------------------|
| Web framework      | ASP.NET Core 8 **Razor Pages** |
| Auth               | ASP.NET Core **Identity** |
| DB           | **EF Core** + SQL server / Azure SQL |
| File storage       | **Azure Blob Storage**       |
| UI                 | Bootstrap                    |
| Hosting            | **Azure App Service**        |

## License
This project is licensed under the [Apache License 2.0](./LICENSE).
