# Unit 3, Lesson 5 - I am Going Through Changes ...

At this point you already know how to perform low-level operations with RavenDB. 

In this lesson you will learn how to use the [Changes API](https://ravendb.net/docs/article-page/4.0/csharp/client-api/changes/what-is-changes-api)

## What is the `Changes API`

Changes API is an amazing feature that allows you to receive messages from the server about the events occurred there.

Using the Changes API, you will get notified by the server whenever an event you are interested is fired without polling. Polling is wasteful. Most of the time you spend a lot of time asking the same
question and expecting to get the same answer.

## Exercise: Getting notified when a document changes

In this exercise, you will learn how to get notifications whenever any document is changed.

### Step 1: Create a new project and install the latest `RavenDB.Client` and `System.Reactive.Core` packages

Start Visual Studio and create a new `Console Application Project` named
`BasicsOfChangesAPI`. Then, in the `Package Manager Console`, issue the following
commands:

```powershell
Install-Package RavenDB.Client -Version 4.0.3
Install-Package System.Reactive.Core
Install-Package System.Reactive.Linq
```

Yeap! RavenDB is reactive!

### Step 2: Initialize the `DocumentStore`

As you already know, we will manage the `DocumentStore` using our great friend `DocumentStoreHolder` pattern.  

````csharp
using System;
using Raven.Client;
using Raven.Client.Documents;

namespace BasicsOfChangesAPI
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

### Step 3: Subscribing to changes

Now it's time to subscribe!

````csharp
using System;
using Raven.Client;
using Raven.Client.Documents;

namespace BasicsOfChangesAPI
{
    using static Console;

    class Program
    {
        static void Main(string[] args)
        {
            var subscription = DocumentStoreHolder.Store
                .Changes()
                .ForAllDocuments()
                .Subscribe(change =>
                    WriteLine($"{change.Type} on document {change.Id}"));

            WriteLine("Press any key to exit...");
            ReadKey();

            subscription.Dispose();
        }
    }
}
````

Now every time something changes a document in the server (Put, Delete), your application will get notified. Test it! Change some documents using the Studio and confirm it.

Notice that the change notification include the document (or index) ID and the
type of the operation performed. Put or Delete in the case of documents, most
often. If you want to actually access the document in question, you’ll need to
load it using a session (as you already know).

In a "real world" application, you can take actions, such as notify the user (e.g. using
SignalR if you are running a web application).

## What Changes are Supported by the Changes API?

You can register for notifications on specific documents, all documents with a specific prefix or of a
specific collection, all documents changes, or for updates to indexes.

## Great job! 

You just learned how to use the Changes API.

This is an extremely powerful feature that enables a whole host of interesting scenarios.

**Let's move onto [Lesson 6](../lesson6/README.md).**

