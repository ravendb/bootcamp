# Unit 3, Lesson 5 - Working with Listeners

This is the last lesson of this bootcamp. You already know the basics about how to use RavenDB and you are ready to move on. Please,
note that there is an [extensive documentation](http://ravendb.net/docs) available on-line.

In this last lesson, you will learn how to work with Listeners.

## Listeners?

Yes, Listeners! Using the client API you can define an additional behavior for whenever your code does something with RavenDB.

## Writing a listener

Listeners are simple classes implementing at least one of the following interfaces:

* `IDocumentStoreListener` - called when an entity is stored on the server
* `IDocumentDeleteListener` - called when an entity is deleted on the server
* `IDocumentQueryListener` - called before a query is executed in the server
* `IDocumentConversionListener` - called when converting an entity to a document and vice versa.
* `IDocumentConflictListener` - called when a replication conflict is encountered. Anyway, replication is an important concept that is out of scope of this bootcamp.

By creating a listener you could e.g. prevent deletion of a document based on your business logic. Interesting, huh?

## Exercise: Listening when a document is stored

In this exercise, you will learn how to listen when a document is stored. Something basic but, good enough to understand the concept.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`WorkingWithListeners`. Then, in the `Package Manager Console`, issue the following
commands:

```
Install-Package RavenDB.Client
```

### Step 2: Create a listener

As you know, to create a listener you just need to write a class that implements a listener interface.

````csharp
public class MyDocumentStoreListener : IDocumentStoreListener
{
    public bool BeforeStore(string key,
        object entityInstance, RavenJObject metadata, RavenJObject original)
    {
        Console.WriteLine($"Before storing {key}.");
        var allow = key != "categories/99";
        if (!allow)
            throw  new InvalidOperationException($"'{key}' is not an acceptable id.");

        return false;
    }

    public void AfterStore(string key, object entityInstance, RavenJObject metadata)
    {
        Console.WriteLine($"After storing {key}.");
    }
}
````

This listener prevents your code from storing a document with id `categories/99`.

### Step 3: Initialize the `DocumentStore` and register the listener

As you already know, we will manage the `DocumentStore` using our great friend `DocumentStoreHolder` pattern.  

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

            store.RegisterListener(new MyDocumentStoreListener());

            store.Initialize();

            return store;
        });

    public static IDocumentStore Store =>
        LazyStore.Value;
}
````

At this time, there is an important modification here: we are registering our listener.  Only registered listeners are executed.

### Step 4: Write the model classes

In this exercise we will work only with entities of type `Category`.

````csharp
public class Category
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
````

### Step 5:  Trying to store a document

````csharp
class Program
{
    static void Main(string[] args)
    {
        using (var session = DocumentStoreHolder.Store.OpenSession())
        {
            var document = new Category()
            {
                Id = "categories/99",
                Name = "Forbidden",
                Description = "Forbidden"
            };

            session.Store(document);
            session.SaveChanges();
        }
    }
}
````

Running this code you will get an exception raised by the listener. Great!

## Using listeners to prevent documents of being deleted

You can easily prevent documents from being deleted by creating and registering the following listener:

````csharp
public class PreventDeleteListener : IDocumentDeleteListener
{
	public void BeforeDelete(string key, object entityInstance, RavenJObject metadata)
	{
		throw new NotSupportedException();
	}
}
````

This listener will throw an exception whenever you try to delete a document. You could use any logic you want
to select what documents could be deleted.


## Using listeners to include additional metadata information

You can include additional metadata information easily. In this example we are including the name of the current user.

````csharp
public class AuditStoreListener : IDocumentStoreListener
{
    public bool BeforeStore(string key, object entityInstance,
        RavenJObject metadata,
        RavenJObject original)
    {
        metadata["Last-Modified-By"] = WindowsIdentity.GetCurrent().Name;
        return false;
    }

    public void AfterStore(string key, object entityInstance, RavenJObject metadata)
    {}
}
````

## Great job! That's all.

Wow! You made it! At this point you already know the basics of RavenDB. Congratulations! Again, you can learn more using the [documentation](http://ravendb.net/docs) available on-line.

Thanks for using this bootcamp to learn RavenDB.
