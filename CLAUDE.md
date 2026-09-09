# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

An **ASP.NET Web Forms Web Application** (.NET Framework 4.6.2, C#) scaffolded from the Visual
Studio 2022 "ASP.NET Web Application (.NET Framework) → Web Forms + Individual User Accounts"
template. It is the starting point for Microsoft's **Wingtip Toys** tutorial store.

Current state: still the bare template. Only `Default.aspx`, `About.aspx`, `Contact.aspx`, the
`Account/` identity pages, and `Site.Master` exist. The tutorial's own domain code — `Product` /
`Category` models, `ProductContext`, `Logic/`, `ProductList.aspx`, the shopping cart — has **not**
been added yet.

Repo: `master` branch, remote `ClaraThant/WingtipToys`.

## Toolchain

Windows + Visual Studio 2022 only. This is an old-style (non-SDK) `.csproj` — **`dotnet build`
cannot build it**; use MSBuild.

```bash
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" WingtipToys.sln -p:Configuration=Debug
```

`packages/` is git-ignored, so a fresh clone needs a NuGet restore (Visual Studio does this
automatically on load) before the build resolves.

**There are no tests** and no test project. Verify changes by running the site.

## Running it

Open **`WingtipToys.sln`** in Visual Studio and press F5/Ctrl+F5. IIS Express serves it at
`http://localhost:56038` (https on 44372), configured in `.vs/WingtipToys/config/applicationhost.config`.
**Must be run 32-bit on this machine** — see the RSA error below.

`WingtipToys.csproj.user` sets `<StartAction>CurrentPage</StartAction>`, so F5 launches whichever
page happens to be open in the editor unless a start page is set explicitly (right-click the
`.aspx` → Set As Start Page).

Two failure modes worth knowing:

* **"bin\WingtipToys.dll is not a valid Win32 application"** on F5 means VS has the project open
  in *Open Folder* mode rather than via the `.sln`. Without the solution, the Web Application
  project flavor never loads, there is no IIS Express launch profile, and VS tries to execute the
  output DLL as a program. Reopen `WingtipToys.sln` through File → Open → Project/Solution.
  (`.vs/.../v17/.wsuo` newer than `.suo` is the tell.)
* **"Failed to decrypt using provider 'RsaProtectedConfigurationProvider'. The RSA key container
  could not be opened."** This machine's **64-bit** root config,
  `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Config\web.config`, was modified (an added
  `<appSettings>` entry plus an encrypted `<connectionStrings>` section, presumably for the OASIS
  sites) and the `NetFrameworkConfigurationKey` RSA container cannot be opened by this account.
  Every ASP.NET app on the box inherits that file, so any 64-bit run dies at startup before
  reaching app code.

  The **32-bit** root config (`Framework\v4.0.30319\Config\web.config`) is untouched stock — no
  `<connectionStrings>` at all — so running as a 32-bit process sidesteps it entirely. Fix:
  Tools → Options → Projects and Solutions → Web Projects → uncheck *"Use the 64 bit version of
  IIS Express for web sites and projects"*, exit the running IIS Express, start again. Verified
  working: `/Default.aspx` 301s to `/Default` and returns 200.

  Do **not** edit that machine `web.config` — its encrypted connection strings belong to the OASIS
  sites in the sibling folders. The alternative repair (`aspnet_regiis -pa
  "NetFrameworkConfigurationKey" "<account>"`, elevated) changes machine-level security settings
  and is useless if the key is absent from this machine. Ask before going down that path.

## Database

EF 6 Code First against LocalDB. `DefaultConnection` in `Web.config` points at
`(LocalDb)\MSSQLLocalDB` with `AttachDbFilename=|DataDirectory|\...`, i.e. an `.mdf` created under
`App_Data/` on first use. `App_Data/` is currently empty — the database does not exist until
something first touches a `DbContext` (registering a user, or running a tutorial page). Delete the
files in `App_Data/` to reset. `sqllocaldb.exe` lives under
`C:\Program Files\Microsoft SQL Server\150\Tools\Binn\`.

The tutorial adds a second `DbContext` (`ProductContext`) sharing this same `DefaultConnection`, so
both Identity tables and product tables land in one database.

## Architecture

**Startup is split across two entry points.** `Global.asax.cs` `Application_Start` runs
`RouteConfig.RegisterRoutes` + `BundleConfig.RegisterBundles`; separately, `Startup.cs` is an OWIN
startup class (`[assembly: OwinStartupAttribute]`) whose `Configuration` calls `ConfigureAuth` in
`App_Start/Startup.Auth.cs`. Auth wiring goes in the OWIN half, everything else in `Global.asax`.

**Auth is OWIN cookies, not Forms auth.** `Web.config` sets `<authentication mode="None" />` and
removes the `FormsAuthentication` module. ASP.NET Identity 2.2 provides `ApplicationUser` and
`ApplicationDbContext` (`Models/IdentityModels.cs`) plus `ApplicationUserManager` /
`ApplicationSignInManager` (`App_Start/IdentityConfig.cs`). Do not reach for
`FormsAuthentication.*` or `<membership>`/`<roleManager>` providers — they are explicitly
`<clear />`ed.

**Friendly URLs are on** (`Microsoft.AspNet.FriendlyUrls`, `RedirectMode.Permanent`). Links are
extensionless — `href="~/About"`, not `~/About.aspx` — and a request to `.aspx` gets a **301**
permanent redirect, which browsers cache aggressively. `Site.Mobile.Master` + `ViewSwitcher.ascx`
are the FriendlyUrls mobile view-switching pair.

**Web Application, not Web Site project.** Everything compiles into `bin\WingtipToys.dll` ahead of
time. Consequences when adding files outside Visual Studio:

* Every file must be registered in `WingtipToys.csproj` — `<Compile Include>` for `.cs`,
  `<Content Include>` for `.aspx`/`.ascx`/static assets — or it will not build or serve.
* Pages are a three-file set: `.aspx` (markup), `.aspx.cs` (code-behind), `.aspx.designer.cs`
  (auto-generated control fields). The designer file is regenerated by VS from the markup; add
  controls in the `.aspx` and let it regenerate rather than hand-editing it.
* Code-behind changes require a rebuild; markup-only changes do not.

**Assets and bundling** are split oddly: the style bundle is declared declaratively in
`Bundle.config`, while script bundles are declared in code in `App_Start/BundleConfig.cs`.
`BundleConfig` also maps the `ScriptManager` `"jquery"` name to the local jQuery 3.7 file with a
CDN fallback, which is how `Site.Master`'s `<asp:ScriptManager>` resolves jQuery. Bootstrap 5.2.3
in `Content/` and jQuery in `Scripts/` come from NuGet — treat them as vendored and don't edit.

`Site.Master` is the shell for every page: nav bar, the `MainContent` placeholder, and the
logged-in/logged-out link switch.

## Conventions

Modern C# is fine here — the Roslyn CodeDom provider (`Microsoft.CodeDom.Providers.
DotNetCompilerPlatform`) is installed and `<system.codedom>` is active with `/langversion:default`.

Files are UTF-8 with BOM and CRLF line endings; `.gitattributes` enforces this. Match the file
you are editing.

## Relationship to the surrounding workspace

The parent folder `C:\Web\Projects\kyisin26\` has an `AGENTS.md` covering three UC Davis OASIS
repos (`students.ucdavis.edu`, `Student-Files`, `resources-core`). **WingtipToys is unrelated to
that product** and is a separate git repo. The hard constraints in that file — C# 5 only, stored
procedures for all data access, Enterprise Library, the Azure DevOps pipelines — **do not apply
here**. Do not carry them over.
