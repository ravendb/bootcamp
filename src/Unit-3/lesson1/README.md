# Unit 3, Lesson 1 - Document Metadata

Here we go! Welcome to your first lesson of Unit 3. It's time to learn about
an important RavenDB concept: Metadata.

## What is the Document Metadata?

The Metadata is a place that RavenDB uses to store additional information about the documents.

Every document in RavenDB has metadata attached to it. The metadata, like the
document data, is also stored in JSON format.

You can use it as well if you want or need.

## Exercise: Loading metadata using a document

Again, let's learn by doing.

In this exercise you will learn how to get access to a document metadata.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`GettingMetadata`. Then, in the `Package Manager Console`, issue the following
command:

```Install-Package RavenDB.Client```

This will install the latest RavenDB.Client binaries, which you will need in order
to compile your code.

### Step 2: Initialize the `DocumentStore`

Here we go again. Let's manage the `DocumentStore` using the `DocumentStoreHolder` pattern.  

````csharp
using System;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace GettingMetadata
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

Remember to start the RavenDB server.

### Step 3: Loading the metadata

Now, it's time to load a document metadata.

````csharp
class Program
{
    static void Main()
    {
        using (var session = DocumentStoreHolder.Store.OpenSession())
        {
            var product = session.Load<Product>("products/1");
            RavenJObject metadata = session.Advanced.GetMetadataFor(product);

            foreach (var info in metadata)
            {
                Console.WriteLine($"{info.Key}: {info.Value}");
            }
        }
    }
}

class Product { }
````

What are we doing here? First we are asking for an instance of the document. Then, using this instance
we get the metadata.

This is the output on my machine:

````
Raven-Entity-Name: Products
Raven-Clr-Type: Orders.Product, Northwind
Raven-Last-Modified: 25/07/2016 14:35:55
Last-Modified: 25/07/2016 14:35:55
@etag: 01000000-0000-0001-0000-0000000003D7
````

The property names follow the HTTP Headers conventions. In other words, words are separated
with a dash and the first letter of each word is capitalized. In this example:

* `Raven-Entity-Name` specifies the collection name of the document
* `Raven-Clr-Type` specifies the client side type used to store the document
* `Raven-Last-Modified` and `Last-Modified` ... you know
* `@etag` ... well we will need more time to talk about this property.

### Step 4 (bonus): adding a property to the metadata

Once you have the metadata, you can modify it as you wish. The session tracks changes to both
the document and its metadata, and changes to either of those will cause the document to be
updated on the server once `SaveChanges` method has been called.

````csharp
class Program
{
    static void Main()
    {
        using (var session = DocumentStoreHolder.Store.OpenSession())
        {
            var product = session.Load<Product>("products/1");
            RavenJObject metadata = session.Advanced.GetMetadataFor(product);

            metadata["Last-Modified-By"] = "Oren Eini";
            session.SaveChanges();
        }
    }
}

class Product { }
````

## Some words about `Raven-Entity-Name` property
`Raven-Entity-Name` specifies the name of the document collection. As you will learn soon,
you can change the value of a metadata property. But, you should not try to do this with
`Raven-Entity-Name`.

RavenDB engine does a lot of optimizations based on the collection name, and no support
whatsoever for changing it.

## Exercise: Loading only document metadata

Sometimes you would like to get only the metadata information. It's easy to get with RavenDB.

This exercise picks up right where previous one left off. You will just need to change the `Main` method.

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

## Great job! Onto Lesson 2!

Awesome! You just learned about the metadata concept. You are almost an expert!

**Let's move onto [Lesson 2](../lesson2/README.md).**
