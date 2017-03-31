# Unit 3, Lesson 3 - Performing batch operations using commands!

As you learned in the [previous lesson](../lesson2/README.md), RavenDB
provides a low-level API that you can use to execute not trivial tasks.

In this lesson, you will learn how to use commands to update or delete
a large amount of documents answering a certain criteria.

## The need to batch operations

Sometimes you will need to update or delete a large amount of documents
answering a certain criteria.

Using SQL it would be easy. You just need to use the `UPDATE` and `DELETE`
statements.

````SQL
UPDATE Products SET Price = Price * 1.1 WHERE Discontinued = false;
DELETE Products WHERE Discontinued = true;
````

Anyway, this is not the case when using NoSQL databases, where batch
operations are not supported.

## RavenDB approach for batch operations
Using RavenDB, you can perform batch operations using the `UpdateByIndex`
and `DeleteByIndex` commands, by passing it a query and
an operation definition. RavenDB will perform that operation on the
query results.

It is not easy as performing a simple SQL command, but I think it's easy
enough to be implemented considering the impact of these operations.  

## Exercise: Increasing the `PricePerUnit` for non-discontinued products

In this exercise, you will learn how to increase prices of all
non-discontinued products.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`BatchOperationsWithRavenDB`. Then, in the `Package Manager Console`, issue the following
command:

```Install-Package RavenDB.Client```

### Step 2: Initialize the `DocumentStore`

Here we go again. let's manage the `DocumentStore` using our great friend `DocumentStoreHolder` pattern.  

````csharp
using System;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace BatchOperationsWithRavenDB
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

                var asm = Assembly.GetExecutingAssembly();
                IndexCreation.CreateIndexes(asm, store);

                return store;
            });

        public static IDocumentStore Store =>
            LazyStore.Value;
    }
}
````

We are performing an Assembly scan for indexes. You learned about it in the
Unit-2.

### Step 3: Write the model class

As usual, in this exercise, we will write a model class just with the properties
we need to perform the batch operation.

It's important to mention that in a "real world" application you probably would  
have all the model classes written. Right?

````csharp
public class Product
{
    public int PricePerUnit { get; set; }
    public bool Discontinued { get; set; }
}
````

### Step 4: Writing the index creation task

You will need to perform a query. As you know, RavenDB uses indexes for all
queries. So, before querying, we will need to define an index.

````csharp
public class Products_ByDiscontinued : AbstractIndexCreationTask<Product>
{
    public Products_ByDiscontinued()
    {
        Map = (products) =>
            from p in products
            select new {p.Discontinued};
    }
}
````

This is just a pretty regular index. Nothing special! Note that in a "real world" application
you would probably have already defined an index that you could use instead
of creating a new one.

### Step 5: Performing a batch operation

We now have everything we need to perform a batch operation. Let's do it.

````csharp
class Program
{
    static void Main()
    {
        var commands = DocumentStoreHolder.Store.DatabaseCommands;
        var operation = commands.UpdateByIndex(
            "Products/ByDiscontinued",
            new IndexQuery { Query = "Discontinued:false" },
            new ScriptedPatchRequest
            {
                Script = @"this.PricePerUnit = this.PricePerUnit * 1.1"
            }
            );

        operation.WaitForCompletion();
        Console.WriteLine("All active products had Price per unit increased in 10%");
    }
}
````

Please note that we are using a low-level command here. There is no need to have
a session in this case.

Run it! And check the results. Amazing!

## Before you go ...

You just learned how to update documents in the server without having to load
it in the client side. This is an extremely powerful concept. But, remember
*"With great powers comes great responsibility"*.

Using the `DeleteByIndex` command, which works in a very similar way, you can
delete a lot of documents ... and this can be dangerous.

## Great job! Onto Lesson 4!

Awesome! Now you know how to easily update a large amount of documents!

**Let's move onto [Lesson 4](../lesson4/README.md).**
