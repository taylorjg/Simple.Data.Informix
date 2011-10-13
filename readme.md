# Simple.Data.Informix

This project implements an Informix ADO provider for Simple.Data.

## Recent Changes

- Added lots of unit tests (currently 75 in total)
- Created a nuspec file
- Raised issue #95 against Simple.Data (https://github.com/markrendle/Simple.Data/issues/95)

## Versions Tested

- Informix Dynamic Server Version 7.31.UD5 (on Linux 2.4.20-64GB-SMP)
- IBM Informix Dynamic Server Version 11.50.FC7W3 (on HP-UX B.11.31 U ia64)

Tests were conducted on Windows XP SP3 using IBM Informix Client-SDK 3.50.TC5.

## TODO

- Add support for IQueryPager
- Add support for stored procedures
- Fix problem - System.ArgumentException / "Unknown SQL type - INTERVAL_DAY."
- Add tests for more column data types
- Run all unit tests against both V7 and V11
- Raise an issue against Simple.Data whereby a stack overflow occurs when trying to use .Take() against my provider which currently does not support IQueryPager

## Installation

Obtain and install the IBM Informix Client-SDK.

## Demo Program

```C#
using System;
using Simple.Data;

namespace TestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string connectionString = "Server=...; Database=stores7; User ID=...; Password=...";
            const string providerName = "IBM.Data.Informix";
            var db = Database.Opener.OpenConnection(connectionString, providerName);
            var customer = db.Customers.FindByCustomerNum(101);
            var fname = customer.FName.Trim();
        }
    }
}
```
