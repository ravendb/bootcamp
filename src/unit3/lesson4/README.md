# Unit 3, Lesson 4 - Performing batch operations!

As you learned in the [previous lesson](../lesson3/README.md), RavenDB
provides a low-level API that you can use to execute not trivial tasks.

In this lesson, you will learn how to use operations to update 
a large amount of documents answering a certain criteria.

## The need to batch operations

Sometimes you will need to update or delete a large amount of documents
answering a certain criteria.

Using SQL it would be easy. You just need to use the `UPDATE` and `DELETE`
statements.

```sql
UPDATE Products SET Price = Price * 1.1 WHERE Discontinued = false;
```

Anyway, this is not the case when using NoSQL databases, where set
based operations are not supported.

## RavenDB approach for batch operations

Using RavenDB, the same queries and indexes that are used for data retrieval 
are used for the set based operations. The syntax defining which documents to
work on is exactly the same as you'd specified for those documents to be pulled 
from the store.

## Exercise: Increasing the `PricePerUnit` for non-discontinued products

In this exercise, you will learn how to increase prices of all
non-discontinued products.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`BatchOperationsWithRavenDB`. Then, in the `Package Manager Console`, issue the following
command:

```powershell
Install-Package RavenDB.Client -Version 4.0.3
```

### Step 2: Initialize the `DocumentStore`

Here we go again. let's manage the `DocumentStore` using our great friend `DocumentStoreHolder` pattern.  

````csharp
using System;
using Raven.Client;
using Raven.Client.Documents;

namespace BatchOperationsWithRavenDB
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

### Step 3: Performing a batch operation

We now have everything we need to perform a batch operation. Let's do it.

```csharp
static void Main()
{
    var operation = DocumentStoreHolder.Store
        .Operations
        .Send(new PatchByQueryOperation(@"from Products as p
                                where p.Discontinued = false
                                update
                                {
                                    p.PricePerUnit = p.PricePerUnit * 1.1
                                }"));
    operation.WaitForCompletion();
}
```

RQL for the win!

There is a very good list of examples available on the [RavenDB documentation](https://ravendb.net/docs/article-page/4.0/csharp/client-api/operations/patching/set-based). 


Run it! And check the results. Amazing!

## Before you go ...

You just learned how to update documents in the server without having to load
it in the client side. This is an extremely powerful concept. But, remember
*"With great powers comes great responsibility"*.

## Great job! Onto Lesson 5!

Awesome! Now you know how to easily update a large amount of documents!

**Let's move onto [Lesson 5](../lesson5/README.md).**
