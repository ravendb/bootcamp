# Unit 1, Lesson 3 - Let's Code!

Until now, we were only using the Management Studio. Now it's time to write some code. 

In this lesson you will write some very simple C# programs which load data from the database.

We are using C# here, but you can you Java, Node, Python, Ruby, Go... There different
syntaxes but the main idea remains the same.

This lesson picks up right where [Lesson 2](../lesson2/README.md) left off.

## Exercise 1: Your first RavenDB Client Application

In this exercise we will write a very basic RavenDB client application. You 
can use .NET core or .NET full.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
Northwind. Then, in the `Package Manager Console`, issue the following
command:

```Install-Package RavenDB.Client -Version 4.0.3```

This will install RavenDB.Client binaries, which you will need in order
to compile your code.

You will need to add the `using` namespace at the top of `Program.cs`:

````csharp
using Raven.Client.Documents;
````

### Step 2: Initialize the `DocumentStore`

Start your `Main` method with the following code:

````csharp
var documentStore = new DocumentStore
{ 
    Urls = new [] {"http://localhost:8080"},
    Database = "Northwind"
};

documentStore.Initialize();
````

The document store is the starting point for all your interactions with RavenDB.

> A document store is the main client API object, which establishes and manages the connection channel between an application and a database instance. It acts as the connection manager and also exposes methods to perform all operations that you can run against an associated server instance.
The document store object has an array of URL addresses to the cluster nodes, however it can work against multiple databases that exist there.

> To learn more about the `DocumentStore`, access the [official documentation](https://ravendb.net/docs/article-page/4.0/csharp/client-api/what-is-a-document-store).

### Step 3: Load a document from the server

After creating an instance of the `DocumentStore` we are ready to interact with the
database.

After creating a RavenDB document store, we are ready to use the database server instance it is pointing at. For any operation we want to perform on the DB, we start by obtaining a new Session object from the document store. The Session object will contain everything needed to perform any operation necessary.

````csharp
using (var session = documentStore.OpenSession())
{
    var p = session.Load<dynamic>("products/1-A");
    System.Console.WriteLine(p.Name);
}
````

RavenDB is schema-less. We don't need to create any class to load documents from the server.

> When you want to store data in a relational database, you first define a schema -
a defined structure for the database which says what tables and columns exist and
which data types each columns can hold. With RavenDB, storing and loading data is much
more casual.

Here is the complete code of this exercise.

````csharp
using Raven.Client.Documents;

namespace Northwind
{
    class Program
    {
        static void Main()
        {
            var documentStore = new DocumentStore
            {
                Urls = new[] { "http://localhost:8080" },
                Database = "Northwind"
            };

            documentStore.Initialize();

            using (var session = documentStore.OpenSession())
            {
                var p = session.Load<dynamic>("products/1-A");
                System.Console.WriteLine(p.Name);
            }
        }
    }
}
````

## Understanding the Concept of `Unique Identifiers`
As you probably noted, there is no need to inform the source collection when loading
a document. It is possible because, as you learned in [Lesson 1](../lesson1/README.md), the collection
in RavenDB is just "virtual."

To load a document, you simply need to specify its Id. Any
string can be used as a document Id. RavenDB uses a default convention
where the collection name is used as a prefix followed by a unique value.

The document Id is equivalent for the primary key in a relational system. Unlike
a primary key, which is unique per table, the document Id is unique per database.
This also means that whenever you store a new document using a key that is already used
by a document, that document will get overwritten with the new document data.

> You can learn more about how to work with document Ids reading the [official documentation](https://ravendb.net/docs/article-page/4.0/csharp/client-api/document-identifiers/working-with-document-identifiers)

## Exercise 2: Improving the code with some types

This exercise picks up right where the previous one left off.

In the previous exercise we started loading data from the server without defining
any class. The schema-less nature of RavenDB, combined with the `dynamic` keyword in C#
allows us to work in a completely dynamic world. But for most things, we
do want some structure.

### Step 1: Introducing model classes

We could define a very basic model to handle our product information.

````csharp
public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
}
````

That's good enough if we just want to load partial information of the documents.
But we should always define or model classes as complete as possible.

In this exercise, we can ask the `Studio` to provide type information compatible with
Northwind through `Settings`/`Create Sample Data`/`View C# classes`.
We can even request the model class while editing a document clicking in the caret next to
`Copy to clipboard`, then `Copy as C# class`.

````csharp
public class Product
{
    public string Name { get; set; }
    public string Supplier { get; set; }
    public string Category { get; set; }
    public string QuantityPerUnit { get; set; }
    public float PricePerUnit { get; set; }
    public int UnitsInStock { get; set; }
    public int UnitsOnOrder { get; set; }
    public bool Discontinued { get; set; }
    public int ReorderLevel { get; set; }
}
````

You can see that there really isn't anything special about these classes. There
is no base class, attributes or even virtual members.

### Step 2: Using your model class when interacting with RavenDB

Starting to use model classes is really easy with RavenDB.

````csharp
using (var session = documentStore.OpenSession())
{
    var p = session.Load<Product>("products/1-A");
    System.Console.WriteLine(p.Name);
}
````

The code is same as before but we are using the `Product` class instead of `dynamic`.

It just works!

Here is the complete code for this exercise.

````csharp
using Raven.Client.Documents;

namespace Northwind
{
    class Program
    {
        static void Main()
        {
            var documentStore = new DocumentStore
            {
                Url = "http://localhost:8080",
                DefaultDatabase = "Northwind"
            };

            documentStore.Initialize();

            using (var session = documentStore.OpenSession())
            {
                var p = session.Load<Product>("products/1-A");
                System.Console.WriteLine(p.Name);
            }
        }
    }

    public class Product
    {
        public string Name { get; set; }
        public string Supplier { get; set; }
        public string Category { get; set; }
        public string QuantityPerUnit { get; set; }
        public float PricePerUnit { get; set; }
        public int UnitsInStock { get; set; }
        public int UnitsOnOrder { get; set; }
        public bool Discontinued { get; set; }
        public int ReorderLevel { get; set; }
    }
}
````

## Great job! 

The third lesson is done.

**Let's move on to [Lesson 4](../lesson4/README.md) and learn more about the `DocumentStore`.**
