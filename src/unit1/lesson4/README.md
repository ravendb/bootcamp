# Unit 1, Lesson 4 - The Basics of the DocumentStore

In the previous lessons you have set up RavenDB, explored the Studio, written
some code to connect to RavenDB, pulled data out, and defined typed classes
that allow you to work with RavenDB easily.

In this lesson, you will understand more about an important member of the Client API:
the `DocumentStore`.

## What is the Purpose of the `DocumentStore`?

You've already used the document store in the previous example. Now it is time
to understand its purpose.

````csharp
var documentStore = new DocumentStore
{ 
    Urls = new [] {"http://localhost:8080"},
    Database = "Northwind"
};

documentStore.Initialize();
````

The document store holds the RavenDB URL, the default database, and the certificate
that should be used.

The document store holds all client-side configuration for RavenDB - how we are
going to serialize entities, how to load balance reads, cache sizes, timeouts, 
and much more.

**In typical applications, you will have a single document store per application.**

## Exercise: Moving the `DocumentStore` instance to a singleton class

This exercise picks up right where previous one in the last lesson left off.

What you will do to ensure a single document store in your application is to adopt
a typical initialization pattern.

### Step 1: Making the Document Store Instance Singleton

```csharp
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
```
The use of Lazy ensures that the document store is only created once, without
having to worry about locking or other thread safety issues.

### Step 2: Using the Singleton `DocumentStore` Instance

Now you can improve your code to use the `DocumentStoreHolder`.

```csharp
class Program
{
    static void Main()
    {

        using (var session = DocumentStoreHolder.Store.OpenSession())
        {
            var p = session.Load<Product>("products/1-A");
            System.Console.WriteLine(p.Name);
        }
    }
}
```

## Introducing Conventions

An important RavenDB concept is conventions.

> Conventions are a series of policy decisions that have already been made for you.
> They range from deciding which property holds the document ID, to how the entity
> should be serialized to a document.

A lot of thought and effort was put into ensuring you won't have to touch
the conventions. But you can do it every time you need to.

We will not touch RavenDB conventions right now simply because we don't need
to do it. If you want to know more, you can access the [RavenDB conventions documentation](https://ravendb.net/docs/article-page/latest/csharp/client-api/configuration/conventions).

## Great Job! 

The fourth lesson is done and you know a lot about the `DocumentStore`.

**Let's move on to [Lesson 5](../lesson5/README.md) and learn more about how to load documents.**
