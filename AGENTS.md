# Agent Instructions for Aspire Repository

This repository contains Altemiq.Aspire.Hosting libraries that extend .NET Aspire with additional resource types.

## Key Commands

```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Clean build artifacts
dotnet clean
```

## Version Management

- Aspire version is centrally managed in `Versions.props` (currently 13.3.1)
- Package versions are centrally managed in `Directory.Packages.props` files
- Different package groups (Aspire, AWS, Microsoft, etc.) have separate versioning

## Multi-targeting

Projects target both:
- `net8.0` (current stable)
- `net10.0` (next version)

## Repository Structure

- `src/` - Main library implementations
- `tests/` - Unit and integration tests
- `playground/` - Sample applications demonstrating usage
- Root contains shared configuration files

## CI/CD Pipeline

GitHub Actions workflow handles:
- Testing on Windows and Ubuntu
- Code coverage generation
- NuGet package creation with semantic versioning
- Package publishing to GitHub Packages

## Known Issues and Workarounds

### Logging Generator Conflicts
Some playground projects may encounter compilation errors related to the Microsoft.Extensions.Logging.Generators package when targeting .NET 8.0:

```
error CS0757: A partial method may not have multiple implementing declarations
```

This occurs when using LoggerMessage attributes in projects that also reference the Playground.ServiceDefaults project. The solution has been centralized in the `playground/Directory.Build.targets` file, which applies the fix to all playground projects automatically.