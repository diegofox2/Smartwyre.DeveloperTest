# Smartwyre Developer Test

Sample project for calculating rebates/incentives on sales. Business logic is organized into types, services, and data stores to make it easy to extend.

**Main structure**

- `Smartwyre.DeveloperTest` : Main source project.
  - `Services/` : Calculation implementations and services (for example `RebateService`, `IRebateCalculator`, concrete calculators).
  - `Data/` : Data store interfaces and implementations (`IProductDataStore`, `IRebateDataStore`).
  - `Types/` : Models and DTOs (`Product`, `Rebate`, `CalculateRebateRequest`, `CalculateRebateResult`, `SupportedIncentiveType`, etc.).
- `Smartwyre.DeveloperTest.Runner` : Console runner project for examples and manual runs.
- `Smartwyre.DeveloperTest.Tests` : Unit tests project.

**Overview**
This project calculates rebates according to different incentive types (for example, fixed amount per UOM, fixed percentage, etc.). Responsibilities are separated so adding new calculation rules is straightforward.

**How to add a new calculation**

1. Define the incentive type if needed:

- Add a value to `Types/SupportedIncentiveType.cs` for the new incentive.

2. Implement the calculator:

- Create a new class in `Services/` that implements `IRebateCalculator`.
- Implement the calculation logic in the calculator method (accept the required `Rebate` and `Product`/request data and return `RebateCalculation` or `CalculateRebateResult` according to the existing design).

3. Register/use the new calculator:

- If `RebateService` uses a `switch` or `if` on `SupportedIncentiveType`, add the case to return/use the new implementation.
- If the project uses dependency injection, register the new implementation and resolve it where appropriate.

4. Add unit tests:

- Create tests in `Smartwyre.DeveloperTest.Tests` that cover the expected cases for the new calculator.

5. Update documentation/README (this file) and any examples in `Runner`.

Design tips:

- Keep each calculator focused on a single strategy (Single Responsibility Principle).
- Avoid accessing data stores directly from the calculator: pass required data as parameters (product, rebate, quantity, etc.).
- Reuse existing types in `Types/` and follow the `IRebateCalculator` and `IRebateService` interfaces for consistency.

**How to run the project**
Requirements: .NET SDK 8+ installed.

- Build the solution:

```bash
dotnet build Smartwyre.DeveloperTest.sln
```

- Run the console runner project:

```bash
dotnet run --project Smartwyre.DeveloperTest.Runner/Smartwyre.DeveloperTest.Runner.csproj
```

- Run unit tests:

```bash
dotnet test Smartwyre.DeveloperTest.Tests/Smartwyre.DeveloperTest.Tests.csproj
```

There are workspace tasks configured (for example `build`, `publish`, `watch`) that can be used from the development environment.

**Quick example: adding a fixed-percentage calculator**

1. Add `FixedPercentageRebateCalculator : IRebateCalculator` under `Services/`.
2. Implement the logic: apply the percentage to `Product` price \* quantity and return a `RebateCalculation`.
3. Add the `SupportedIncentiveType.FixedPercentage` case and register/consume it from `RebateService`.

**Notes**
This README is a quick guide; review the files in `Services/` and `Types/` to see exact signatures and existing example implementations.
