# Onboarder Modernization

Modernization case study of a legacy onboarding/offboarding MVC app into a clean, cloud-ready .NET 9 / Blazor system.

On one side: **legacy ASP.NET MVC + jQuery horror**.  
On the other: a **modern, layered, cloud-ready** implementation (work in progress).

The goal is to show how a realistic, messy internal tool can be evolved into something maintainable and production-ready.

---

## 🎯 What this repo is about

This repository simulates a common real-world scenario:

- A company builds an **internal onboarding/offboarding system**.
- They choose **ASP.NET MVC, Razor views, jQuery, one giant `site.js`, EF Core glued into controllers**.
- Over time, the app becomes hard to maintain and impossible to extend
- A modernization effort is started: **Blazor, clean architecture, proper EF Core usage, AWS-ready provisioning pipeline**

This repo contains **both sides**:

- `legacy/` - intentionally bad, but realistic, ASP.NET Core MVC app.
- `modern/` - planned modern implementation using Blazor, API layer, EF Core migrations, and cloud integration.

---

## 🧱 Project structure

```text
onboarder-modernization/
├─ legacy/
│  ├─ LegacyOnboarder.sln
│  └─ src/
│     └─ LegacyOnboarder/
│        ├─ Controllers/
│        │  └─ AdminController.cs        # God controller: onboarding, offboarding, provisioning, everything
│        ├─ Models/
│        │  ├─ AppDbContext.cs           # EF Core DbContext living in Models (on purpose)
│        │  ├─ RequestRecord.cs          # DB entity + DTO + view model hybrid
│        │  ├─ RequestStatus.cs
│        │  ├─ ProvisioningTask.cs
│        │  └─ ProvisioningStatus.cs
│        ├─ Views/
│        │  └─ Admin/
│        │     ├─ Index.cshtml           # Table + modal in one view
│        │     └─ _ProvisioningTasks.cshtml
│        └─ wwwroot/
│           └─ js/
│              └─ site.js                # One giant jQuery file handling everything
└─ modern/
   └─ (planned Blazor + API solution)
