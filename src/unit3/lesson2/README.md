# Unit 3, Lesson 2 - Document Metadata

Here we go! Welcome to second lesson of Unit 3. It's time to learn about
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

Remember to start the RavenDB server.

### Step 3: Loading the metadata

Now, it's time to load a document metadata.

````csharp
static void Main()
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var product = session.Load<Product>("products/1-a");
        var metadata = session.Advanced.GetMetadataFor(product);

        foreach (var info in metadata)
        {
            Console.WriteLine($"{info.Key}: {info.Value}");
        }
    }
}

class Product { }
````

What are we doing here? First we are asking for an instance of the document. Then, using this instance
we get the metadata.

This is the output on my machine:

````
@collection: Products
@change-vector: A:96-ZzT6GeIVUkewYXBKQ6vJ9g
@id: products/1-A
@last-modified: 2018-02-28T11:21:24.1425879Z
````

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
            var metadata = session.Advanced.GetMetadataFor(product);

            metadata["last-modified-by"] = "Oren Eini";
            session.SaveChanges();
        }
    }
}

class Product { }
````

## Some words about `@collection` property
`@collection` specifies the name of the document collection. As you will learn soon,
you can change the value of a metadata property. But, you should not try to do this with
`@collection`.

RavenDB engine does a lot of optimizations based on the collection name, and no support
whatsoever for changing it.

## Exercise: Loading only document metadata

Sometimes you would like to get only the metadata information. It's easy to get with RavenDB.

This exercise picks up right where previous one left off. You will just need to change the `Main` method.

````csharp
static void Main()
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var command = new GetDocumentsCommand("products/1-a", null, metadataOnly: true);
        session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
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

Don't be afraid of these new types. Take time to get confortable with it.

## Great job! Onto Lesson 3!

Awesome! You just learned about the metadata concept. You are almost an expert!

**Let's move onto [Lesson 3](../lesson3/README.md).**
