# Unit 3, Lesson 2 - Commands!

This has been a long journey, right? You already know the basics of RavenDB. But
there are a lot of specifics that can help you to create amazing solutions.

In this lesson you will learn about RavenDB Commands. For the most part, that is 
something you rarely need to use. But is good to know that this is avaiable, just 
in case.

## What are RavenDB Commands?

Commands are a set operations that can be used to manipulate data and change 
configuration on a server. But, wait a minute! Isn't exactly what you do using 
the `session` object?

The `session` is a high level interface to RavenDB which provides the identity map
and Linq queries. But if you want do something in the low-level, then you should 
start using the commands.

There are a exaustive list of RavenDB commands avaiable in the [official documentation](https://ravendb.net/docs/article-page/3.5/csharp/client-api/commands/what-are-commands).

## First-time using RavenDB Commands

You already used a command in the [previous lesson](../lesson1/README.md) to get
the metadata information from the server. Let's remember it.

````csharp
static void Main()
{
    var commands = DocumentStoreHolder.Store.DatabaseCommands;
    var metadata = commands.Head("products/1").Metadata;
    foreach (var info in metadata)
    {
        Console.WriteLine($"{info.Key}: {info.Value}");
    }
}
````
The `Head` method expose a low level way to check if a potentialy large document
exists, without loading it.

## Exercise: Adding a order's line into a order document without loading the entire document
Consider the document `orders/816` from the Northwind database.

````csharp
{
    "Company": "companies/37",
    "Employee": "employees/3",
    "OrderedAt": "1998-04-30T00:00:00.0000000",
    "RequireAt": "1998-05-28T00:00:00.0000000",
    "ShippedAt": "1998-05-06T00:00:00.0000000",
    "ShipTo": {
        "Line1": "8 Johnstown Road",
        "Line2": null,
        "City": "Cork",
        "Region": "Co. Cork",
        "PostalCode": null,
        "Country": "Ireland"
    },
    "ShipVia": "shippers/2",
    "Freight": 81.73,
    "Lines": [
        {
            "Product": "products/34",
            "ProductName": "Sasquatch Ale",
            "PricePerUnit": 14,
            "Quantity": 30,
            "Discount": 0
        },
        {
            "Product": "products/40",
            "ProductName": "Boston Crab Meat",
            "PricePerUnit": 18.4,
            "Quantity": 40,
            "Discount": 0.1
        },
        {
            "Product": "products/41",
            "ProductName": "Jack's New England Clam Chowder",
            "PricePerUnit": 9.65,
            "Quantity": 30,
            "Discount": 0.1
        }
    ]
}
```` 

What if you want to add a order's line? Would we need to load the entire document? No!

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`UsingCommands`. Then, in the `Package Manager Console`, issue the following 
command: 

```Install-Package RavenDB.Client```

### Step 2: Initialize the `DocumentStore` 

Here we go again. let's manage the `DocumentStore` using our great friend `DocumentStoreHolder` pattern.  

````csharp
using System;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace UsingCommands
{
    public static class DocumentStoreHolder
    {
        private static readonly Lazy<IDocumentStore> LazyStore =
            new Lazy<IDocumentStore>(() =>
            {
                var store = new DocumentStore
                {
                    Url = "http://localhost:8080",
                    DefaultDatabase = "Northwind"
                };

                store.Initialize();

                return store;
            });

        public static IDocumentStore Store =>
            LazyStore.Value;
    }
}
````

Remember to have started the RavenDB server.

### Step 3: Update the document using a PatchRequest

It's easy to change the document. 

````csharp
static void Main()
{
    var commands = DocumentStoreHolder.Store.DatabaseCommands;

    var requestToAddLine = new PatchRequest
    {
        Type = PatchCommandType.Add,
        Name = "Lines",
        Value = RavenJToken.FromObject(new
        {
            Product = "products/1",
            ProductName = "Chai",
            PricePerUnit = 18,
            Quantity = 1,
            Discount = 0
        })
    };

    commands.Patch(
        "orders/816",
        new [] { requestToAddLine }
    );
}
````

In this example, you used the `Patch` command which performs partial document updates without having to load, 
modify, and save a full document. 

Learn about the `Patch` command reading the [official documentation](https://ravendb.net/docs/article-page/latest/csharp/client-api/commands/patches/how-to-work-with-patch-requests)

## Great job! Onto Lesson 3!

Awesome! You just learned about the commands. 

**Let's move onto [Lesson 3](../lesson3/README.md).**
