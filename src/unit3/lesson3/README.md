# Unit 3, Lesson 3 - Getting started with Operations and Commands

You already know the basics of RavenDB. But
there are a lot of specifics that can help you to create amazing solutions.

In this lesson you will learn about RavenDB Operations and Commands. For the most part, that is
something you rarely need to use. But is good to know that this is available, just
in case.

## What are RavenDB Operations and Commands?

Using Operations and Commands you can manipulate data and change
configuration on a server. But isn't this exactly what you do using
the `session` object?

The `session` is a high level interface to RavenDB which provides the identity map
and LINQ queries. But if you want do something in the low-level, then you should
start using the Operations. Also, there are operations for Database Maintenance, Server Maintenance and Patching

There is an exhaustive list of RavenDB operations available in the [official documentation](https://ravendb.net/docs/article-page/4.0/csharp/client-api/operations/what-are-operations).

## First-Time Using RavenDB Operations and Commands

You already used a command in the [previous lesson](../lesson1/README.md) to get
the metadata information from the server. Let's remember it.

````csharp
static void Main()
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var command = new GetDocumentsCommand(
            "products/1-a", null, metadataOnly: true);
        session.Advanced.RequestExecutor.Execute(
            command, session.Advanced.Context);
        var result = (BlittableJsonReaderObject)command.Result.Results[0];
        var metadata = (BlittableJsonReaderObject)result["@metadata"];

        foreach (var propertyName in metadata.GetPropertyNames())
        {
            metadata.TryGet<object>(propertyName, out var value);
            Console.WriteLine($"{propertyName}: {value}");
        }
    }
}
````

The `GetDocumentsCommand` method exposes a low-level way to get the metadata from a potentially large document without loading it.

## Exercise: Adding an order's line into an order document without loading the entire document

Consider the document `orders/816-a` from the Northwind database.

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

What if you want to add an order's line? Would we need to load the entire document? No!

### Step 1: Create a new project and install `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`UsingCommands`. Then, in the `Package Manager Console`, issue the following
command:

```powershell
Install-Package RavenDB.Client -Version 4.0.3
```

### Step 2: Initialize the `DocumentStore`

Let's manage the `DocumentStore` using our great friend `DocumentStoreHolder` pattern.  

````csharp
using System;
using Raven.Client;
using Raven.Client.Documents;

namespace GettingMetadata
{
    public static class DocumentStoreHolder
    {
        private static readonly Lazy<IDocumentStore> LazyStore =
            new Lazy<IDocumentStore>(() =>
            {
                var store = new DocumentStore
                {
                    Urls = new[] { "http://localhost:8080" },
                    Database = "Northwind"
                };

                return store.Initialize();
            });

        public static IDocumentStore Store =>
            LazyStore.Value;
    }
}
````

Remember to have started the RavenDB server.

### Step 3: Update the document using a `PatchCommand`

It's easy to change the document.

We can use the untyped API, which is useful when you don't have 
C# types to represent your model.

````csharp
static void Main()
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        session.Advanced.Defer(new PatchCommandData(
            id: "orders/816-A",
            changeVector: null,
            patch: new PatchRequest
            {
                Script = "this.Lines.push(args.NewLine)",
                Values =
                {
                    {
                        "NewLine", new 
                        {
                            Product = "products/1-a",
                            ProductName = "Chai",
                            PricePerUnit=18M,
                            Quantity=1,
                            Discount=0
                        }
                    }
                }

            },
            patchIfMissing: null));

        session.SaveChanges();
    }
}
````

In this example, you used the `Patch` command which performs partial document updates without having to load,
modify, and save a full document. The `Script` needs to be in JavaScript.  

You can use the typed version as well.

```csharp
static void Main()
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        session.Advanced.Patch<Order, OrderLine>("orders/816-A",
            x => x.Lines,
            lines => lines.Add(new OrderLine
            {
                Product = "products/1-a",
                ProductName = "Chai",
                PricePerUnit = 18M,
                Quantity = 1,
                Discount = 0
            }));
                
        session.SaveChanges();
    }
}
```

This version is a lot easier. Right?

## Great job! Onto Lesson 4!

Awesome! 

If you want to know more about Patching we recomend you to use the [documentation](https://ravendb.net/docs/article-page/4.0/csharp/client-api/operations/patching/single-document)

**Let's move onto [Lesson 4](../lesson4/README.md).**
