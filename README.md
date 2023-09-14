
---

# NUBAN Generation Library

## Introduction

This library provides a simple and efficient implementation of the Central Bank of Nigeria's (CBN) revised standards on Nigeria Uniform Bank Account Number (NUBAN) generation for banks and other financial institutions. The NUBAN system is designed to create unique and consistent bank account numbers in Nigeria.

### CBN Revised NUBAN Generation Standards

For a detailed understanding of the CBN's revised NUBAN generation standards, please refer to the official CBN document: [CBN Revised NUBAN Generation Standards (PDF)](https://www.cbn.gov.ng/Out/2020/PSMD/REVISED%20STANDARDS%20ON%20NIGERIA%20UNIFORM%20BANK%20ACCOUNT%20NUMBER%20(NUBAN)%20FOR%20BANKS%20AND%20OTHER%20FINANCIAL%20INSTITUTIONS%20.pdf).

## Features

- Generate valid NUBAN account numbers based on the provided serial number and bank code.
- Validate NUBAN account numbers for accuracy.

## Installation

To use this library in your .NET projects, you can install it via NuGet Package Manager. Use the following command:

```bash
dotnet add package NUBAN 
```
or 

```bash
NuGet\Install-Package NUBAN 
```

## Usage

### Generating a NUBAN Account Number

To generate a NUBAN account number, you can use the `CreateAccount` method. Provide the serial number and bank code as input parameters.

```csharp
using YourPackageName;

// ...

string serialNumber = "123456789";
string bankCode = "012";
string nuban = NigerianNubanUtility.CreateAccount(serialNumber, bankCode);
Console.WriteLine($"Generated NUBAN: {nuban}");
```


### Validating a NUBAN Account Number

To validate a NUBAN account number, you can use the `IsBankAccountValid` method. Provide the account number and bank code as input parameters.

```csharp
using NUBAN;

// ...

string accountNumber = "0123456789";
string bankCode = "012";
bool isValid = NUBANUtility.IsBankAccountValid(accountNumber, bankCode);
Console.WriteLine($"Is Valid NUBAN: {isValid}");
```

### infer bank(s) from account number
```csharp

using NUBAN;

// Example usage
string accountNumber = "0000214579";
string bankCode = "044";

static List<Bank> Banks = new List<Bank>
{
    new Bank { Name = "ACCESS BANK", Code = "044" },
    new Bank { Name = "CITIBANK", Code = "023" },
    // Add other banks here
};

List<Bank> inferredBanks = GetAccountBanks(accountNumber, Banks);

if (inferredBanks.Count > 0)
{
    Console.WriteLine("Valid Banks:");
    foreach (var bank in inferredBanks)
    {
        Console.WriteLine($"Name: {inferredBanks.Name}, Code: {inferredBanks.Code}");
    }
}
else
{
    Console.WriteLine("No banks found for the account number.");
}


static List<Bank> GetAccountBanks(string accountNumber)
{
    List<Bank> validBanks = new List<Bank>();

    foreach (var bank in Banks)
    {
        if (NUBANUtility.IsBankAccountValid(accountNumber, bank.Code))
        {
            validBanks.Add(bank);
        }
    }

    return validBanks;
}

````

## :star2: Whats New 

### 1. Get Up to date banks in Nigeria
```csharp
// Example usage
using NUBAN;

Console.WriteLine("=========All Nigerian Banks=====================");


foreach (var bank in await NUBANUtility.GetBanksAsync())
{
    Console.WriteLine($"{bank.name} - {bank.code}");
}

````

### 2. Infer Bank(s) of an account number
```csharp
// Example usage
using NUBAN;

var accountNumber = "0589902233";

Console.WriteLine($"Banks for {accountNumber}: ");

foreach (var bank in await NUBANUtility.InferBanksAsync(accountNumber))
{
    Console.WriteLine($"{bank.name} - {bank.code}");
}


````

## Versioning

This library follows Semantic Versioning (SemVer). The version number is specified in the project file (`.csproj`) and is used when creating NuGet packages.

## License

This library is provided under the [MIT License](LICENSE.txt).

## Contributing

Contributions and feedback are welcome! Please feel free to open issues or submit pull requests on our GitHub repository.

## Author

[Chidiebube](https://github.com/Chidiebube-Onah/)

## Acknowledgments

This library was created based on the [CBN's](https://www.cbn.gov.ng/) revised NUBAN generation standards.

---
