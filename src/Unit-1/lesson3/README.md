# Unit 1, Lesson 3 - Basics of the `DocumentStore`

In the previous lessons you have set up RavenDB, explored the Studio, written
some code to connect to RavenDB, pulled data out and defined typed classes
that allow you to work with RavenDB more easily.

In this lesson, you will understand more about an important member of the Client API:
the `DocumentStore`.

## What is the purpose of the `DocumentStore`?

You've already used the document store in the previous example. Now it is time
to understand its purpose.

````csharp
var documentStore = new DocumentStore
{
    Url = "http://localhost:8080",
    DefaultDatabase = "Northwind"
};

documentStore.Initialize();
````

The document store holds the RavenDB URL, the default database and the credentials
that should be used.

The document store holds all client-side configuration for RavenDB - how we are
going to serialize entities, how to handle failure scenarios, what sort of caching
strategy to use, and much more.

**In  typical applications, you shall have a single document store per application.**

## Exercise: Moving the `DocumentStore` instance to a singleton class

This exercise picks up right where previous one, in the previous lesson, left off.

What you will do to ensure a single document store in your application is to adopt
a typical initialization pattern.

### Step 1: Making the document store instance singleton

````csharp
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

            return store.Initialize();
        });

    public static IDocumentStore Store =>
        LazyStore.Value;
}
````

The use of Lazy ensures that the document store is only created once, without
having to worry about locking or other thread safety issues.

### Step 2: Using the singleton `DocumentStore` instance

Now, you can improve your code to use the `DocumentStoreHolder`.

````csharp
class Program
{
    static void Main()
    {

        using (var session = DocumentStoreHolder.Store.OpenSession())
        {
            var p = session.Load<Product>("products/1");
            System.Console.WriteLine(p.Name);
        }
    }
}
````

## Introducing Conventions

An important RavenDB concept is conventions.

> Conventions are a series of policy decisions that have already been made for you.
Those range from deciding which property holds the document id to how the entity
should be serialized to a document.

A lot of thought and effort was put into ensuring you will have no need to touch
the conventions. But you can do it every time you need.

We will not touch RavenDB conventions right now. Simply because we don't need
to do it. If you want to know more, you can access the [RavenDB conventions
documentation](https://ravendb.net/docs/article-page/latest/csharp/client-api/configuration/conventions/what-are-conventions).

## Connection Strings

You might have noticed that when we defined the document store so far, we
have done so using hard code URL and database. It is easy, but not practical.

Different environments will use different URLs, databases and credentials. The
good news is that you probably know how to deal with it.

## Exercise: Defining and using a connection string

Again, this exercise picks up right where the previous one left off.

### Step 1: Defining the connection string in `app(web).config`

Add or edit the `app.config` file (we are using a console application). Then,
modify its content adding the `connectionStrings` section.

````xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <connectionStrings>
      <add name="RavenDB" connectionString="Url=http://localhost:8080;Database=Northwind"/>
    </connectionStrings>
</configuration>
````

### Step 2: Using the connection string in the `DocumentStore` initialization

With a connectionString properly defined in the configuration file, you just need
to specify the key during `DocumentStore` initialization.

````csharp
public static class DocumentStoreHolder
{
    private static readonly Lazy<IDocumentStore> LazyStore =
        new Lazy<IDocumentStore>(() =>
        {
            var store = new DocumentStore
            {
                ConnectionStringName="RavenDB"
            };

            return store.Initialize();
        });

    public static IDocumentStore Store =>
        LazyStore.Value;
}
````
> If you want to check what other parameters you can pass in the connection string to the document store please read the following [article](http://ravendb.net/docs/article-page/latest/csharp/client-api/setting-up-connection-string).

## Great job! Onto Lesson 4!

Awesome! The third lesson is done and you know a lot about the `DocumentStore`.

**Let's move onto [Lesson 4](../lesson4/README.md) and learn more about how to load documents.**
