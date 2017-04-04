# Unit 2, Lesson 3 - Multi-map indexes

In the [previous lesson](../lesson2/README.md), you learned how to create
indexes with a single map function, and it is going to operate on a
single collection.

In this lesson you will learn how to create `multi-map indexes`.

## Why to create a `Multi-map` index?

Using the Northwind database, assume you need to search for a particular
person. It could be an employee, a company's contact or a supplier's
contact. How could you do that? Multiple queries? Using RavenDB you can
do that using a single query with an index that maps multiple collections.

## Exercise: Creating your first multi-map index using the C\# API

As you learned in the [previous lesson](../lesson2/README.md), you can create indexes using the
`RavenDB Management Studio` or using the C# API.

In this exercise you will learn how to create a multi-map index using the C# API and you
will use the `Northwind` database. You will write a program that allows the user to
search people from multiple collections (employees, companies and suppliers).

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`MultimapIndexes`. Then, in the `Package Manager Console`, issue the following
command:

```Install-Package RavenDB.Client```

This will install the latest RavenDB.Client binaries, which you will need in order
to compile your code.

Then you will need to add the `using` name space at the top of the `Program.cs`:

````csharp
using Raven.Client.Document;
````

### Step 2: Write the model classes

As you learned, you don't need to write "complete" model classes when you are only reading
from the database. For this lesson, you will just need the names and ids.

````csharp
public class Employee
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class ContactClass
{
    public string Name { get; set; }    
}

public class Company
{
    public string Id { get; set; }
    public ContactClass Contact { get; set;  }
}

public class Supplier
{
    public string Id { get; set; }
    public ContactClass Contact { get; set; }
}
````

### Step 3: Write the `Multi-map index definition`

Every time you create a multi-map index, the index definition class inherits from `AbstractMultiMapIndexCreationTask`.

````csharp
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace MultimapIndexes
{
    public class People_Search : AbstractMultiMapIndexCreationTask<People_Search.Result>
    {
        public class Result
        {
            public string SourceId { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }

        public People_Search()
        {
            AddMap<Company>(companies =>
                from company in companies
                select new Result
                {
                    SourceId = company.Id,
                    Name = company.Contact.Name,
                    Type = "Company's contact"
                }
                );

            AddMap<Supplier>(suppliers =>
                from supplier in suppliers
                select new Result
                {
                    SourceId = supplier.Id,
                    Name = supplier.Contact.Name,
                    Type = "Supplier's contact"
                }
                );

            AddMap<Employee>(employees =>
                from employee in employees
                select new Result
                {
                    SourceId = employee.Id,
                    Name = $"{employee.FirstName} {employee.LastName}",
                    Type = "Employee"
                }
                );

            Index(entry => entry.Name, FieldIndexing.Analyzed);

            Store(entry => entry.SourceId, FieldStorage.Yes);
            Store(entry => entry.Name, FieldStorage.Yes);
            Store(entry => entry.Type, FieldStorage.Yes);
        }
    }
}
````

You can define as many `map functions` as you need. Each map function is defined using
the `AddMap` method and has to produce the same output type. The "source" collection is specified
by the generic parameter type you specify in the `AddMap` function.   

The `Index` method here was used to mark the `Name` class as `Analyzed`
which enables full text search with this field.

The `Store` method was used to enable projections. We will talk more about
that in this lesson.

### Step 4: Initialize the `DocumentStore` and register the index on the server

Again, let's do it using our good friend pattern `DocumentStoreHolder`.

````csharp
using System;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace MultimapIndexes
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

In the [previous lesson](../lesson2/README.md), you learned how to register
each index definition individually. Anyway, it's a lot easier to ask to the
client API to find all indexes classes automatically and send them altogether to the server.
You can do that, using the `IndexCreation.CreateIndexes` method.

The `DocumentStoreHolder` is the perfect place to registering indexes on the
server.

### Step 5: Write the search utility function

Here we have a very special piece of code.

````csharp
public static IEnumerable<People_Search.Result> Search(
    IDocumentSession session,
    string searchTerms
    )
{
    var results = session.Query<People_Search.Result, People_Search>()
        .Search(r => r.Name, searchTerms, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards)
        .ProjectFromIndexFieldsInto<People_Search.Result>()
        .ToList();

    return results;
}
````

The `Search` method allow us to perform advanced querying using all the power
of the Lucene engine. Here we are only accepting a list of terms and projecting
the results directly from index stored values into a common result format.

### Step 6: Use the `Search` utility function

````csharp
static void Main(string[] args)
{
    Console.Title = "Multi-map sample";
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        while (true)
        {
            Console.Write("\nSearch terms: ");
            var searchTerms = Console.ReadLine();

            foreach (var result in Search(session, searchTerms))
            {
                Console.WriteLine($"{result.SourceId}\t{result.Type}\t{result.Name}");
            }
        }
    }
}
````

This code asks the user to provide some search terms. Then it displays the
search results.

Not that even if we query against aggregated data the map-reduce index have
done the pre-calculations in advance so we are not doing any full table scans,
just a *O(logN)* operation

![creating new index](images/unit2-multi-map-output.png)

## Some words about projections

In the previous exercise you created an inner class named `Result`. Right? This
class was used to provide projections. Projections are a way to collect several
fields from a document, instead of working with the whole document.

In the index definition, we instructed RavenDB to store the fields in the index. So,
when querying, the documents will not be loaded from storage and all data will come
directly from the index.   

## Great job! Onto Lesson 4!

Awesome! You just learned how to create multi-map indexes using the RavenDB C# API.
Before you proceed, I strongly recommend that you try creating a multi-map index
using the `RavenDB Management Studio`. You will see how easy it is.

**Let's move onto [Lesson 4](../lesson4/README.md) **
