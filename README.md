# EntityChange

Library to compare two entity object graphs detecting changes

[![Build status](https://ci.appveyor.com/api/projects/status/69cj4wn5o3vlmigm?svg=true)](https://ci.appveyor.com/project/LoreSoft/entitychange)

[![NuGet Version](https://img.shields.io/nuget/v/EntityChange.svg?style=flat-square)](https://www.nuget.org/packages/EntityChange/)

## Download

The EntityChange library is available on nuget.org via package name `EntityChange`.

To install EntityChange, run the following command in the Package Manager Console

    PM> Install-Package EntityChange

More information about NuGet package available at
<https://nuget.org/packages/EntityChange>

## Development Builds


Development builds are available on the myget.org feed.  A development build is promoted to the main NuGet feed when it's determined to be stable.

In your Package Manager settings add the following package source for development builds:
<http://www.myget.org/F/loresoft/>

## Features

- Compare complete entity graph including child entities, collections and dictionaries
- Collection compare by index or element equality
- Dictionary compare by key
- Custom value string formatter
- Custom entity equality compare
- Markdown or Html change report formatter

## Configuration

Configure the Contact properties and collections.

```c#
EntityChange.Configuration.Default.Configure(config => config
    .Entity<Contact>(e =>
    {
        // set the FirstName display name
        e.Property(p => p.FirstName).Display("First Name");
        // compare the Roles collection by string equality
        e.Collection(p => p.Roles)
            .CollectionComparison(CollectionComparison.ObjectEquality)
            .ElementEquality(StringEquality.OrdinalIgnoreCase);
        // set how to format the EmailAddress entity as a string
        e.Collection(p => p.EmailAddresses).ElementFormatter(v =>
        {
            var address = v as EmailAddress;
            return address?.Address;
        });
    })
    .Entity<EmailAddress>(e =>
    {
        e.Property(p => p.Address).Display("Email Address");
    })
);
```

## Comparison

Compare to Contact entities

```c#
// create comparer using default configuration
var comparer = new EntityComparer();

// compare original and current instances generating change list 
var changes = comparer.Compare(original, current).ToList();
```

## Change Report

Sample output from the `MarkdownFormatter`

**OUTPUT** 

* Removed `Administrator` from `Roles`
* Changed `Email Address` from `user@Personal.com` to `user@gmail.com`
* Added `user@home.com` to `Email Addresses`
* Changed `Status` from `New` to `Verified`
* Changed `Updated` from `5/17/2016 8:51:59 PM` to `5/17/2016 8:52:00 PM`
* Changed `Zip` from `10026` to `10027`
* Changed `Number` from `888-555-1212` to `800-555-1212`
* Added `Blah` to `Categories`
* Changed `Data` from `1` to `2`
* Changed `Data` from `./home` to `./path`



