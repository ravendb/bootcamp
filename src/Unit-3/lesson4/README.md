# Unit 3, Lesson 4 - I am going through changes ...

At this point you already know how to perform low-level operations with RavenDB. Nice!

In this lesson, you will learn how to uses the [Changes API](http://ravendb.net/docs/article-page/3.5/csharp/client-api/changes/what-is-changes-api)

## What is the `Changes API`

Changes API is an amazing feature that allows you to receive messages from the server about the events occurred there.

Using the Changes API, you will get notified by the server whenever an event you are interested is fired without polling. Polling is wasteful, most of the time you spend a lot of time asking the same
question and expecting to get the same answer.

## Exercise: Getting notified when a document changes

In this exercise, you will learn how to get notifications whenever any document is changed.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`BasicsOfChangesAPI`. Then, in the `Package Manager Console`, issue the following 
commands: 

```
Install-Package RavenDB.Client
Install-Package System.Reactive.Core
```

Yeap! RavenDB is reactive!

### Step 2: Initialize the `DocumentStore` 

As you already know, we will manage the `DocumentStore` using our great friend `DocumentStoreHolder` pattern.  

````csharp
using System;
using Raven.Client;
using Raven.Client.Document;

namespace BasicsOfChangesAPI
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

### Step 3: Subscribing changes

Now, it's time to subscribe!

````csharp
using System;
using Raven.Client;
using Raven.Client.Document;

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
                .Subscribe(change => WriteLine($"{change.Type} on document {change.Id}"));

            WriteLine("Press any key to exit...");
            ReadKey();

            subscription.Dispose();
        }
    }
}
````

It is amazing! Now, everytime something changes a document in the server (Put, Delete), your application will get notified. Test it! Change some documents using the Studio and confirm it.

Notice that the change notification include the document (or index) id and the
type of the operation performed. Put or Delete in the case of documents, most
often. If you want to actually access the document in question, you’ll need to
load it using a session (as you already knows).

In a real application, you can take actions, such as notify the user (using
SignalR if we are running in web application, for example). 

## What changes are supported by the Changes API?

You can register for notifications on specific documents, all documents with a specific prefix or of a
specific collection, for all documents changes or for updates to indexes.

## Great job! Onto Lesson 4!

You just learned how to use the Changes API. 

This is a extremelly powerful feature that enables an whole host of interesting scenarios. 

**Let's move onto [Lesson 5](../lesson5/README.md).**



