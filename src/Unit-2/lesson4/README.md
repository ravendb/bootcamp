# Unit 2, Lesson 4 - Mapping(, filtering) and reducing

Hello again!

In the [previous lesson](../lesson3/README.md), you learned how to create
multi-map indexes and that was amazing. Right?

In this lesson you will learn how to create `Map-Reduce indexes`.

## What is Map-Reduce?

In essence, it is just a way to take a big task and divide it into discrete
tasks that can be done in parallel.

A Map-Reduce process is composed of a `Map` function that projects data from
documents into a common output (expected) and a `Reduce` function that performs
a summary operation.

I strongly recommend you to read this [blog post](https://ayende.com/blog/4435/map-reduce-a-visual-explanation).
Go ahead! I will be waiting for you.

Still confused? Let's write some code and make it clearer.

## Exercise: Your first map-reduce index

I think the best way to learn about Map-Reduce indexes is to write some code.

In this first exercise let's perform a very simple task. Let's just
count the number of products for each category.

Again, let's do it using the C# API.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`MapReduceIndexes`. Then, in the `Package Manager Console`, issue the following
command:

```Install-Package RavenDB.Client```

This will install the latest RavenDB.Client binaries, which you will need in order
to compile your code.

Then you will need to add the `using` name space at the top of `Program.cs`:

````csharp
using Raven.Client.Document;
````

### Step 2: Write the model classes

Again and it will never be enough,  you don't need to write "complete" model classes when you are only reading
from the database.

````csharp
class Category
{
    public string Name { get; set; }
}

class Product
{
    public string Category { get; set; }
}
````

Yes! Everything we need is here. In the `Product` documents, the category
is specified by the Id in the `Category` property.

### Step 3: Write the `Map-Reduce index definition`

One way to create a Map-Reduce index  definition is inheriting from `AbstractIndexCreationTask`.

In the [previous lesson](../lesson3/README.md), you learned how to create multi-map indexes.
It's important you know that you can combine the power of multi-map with map-reduce by
providing multiple map functions.

````csharp
public class Products_ByCategory : AbstractIndexCreationTask<Product, Products_ByCategory.Result>
{
    public class Result
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }

    public Products_ByCategory()
    {
        Map = products =>
            from product in products
            select new
            {
                Category = product.Category,
                Count = 1
            };

        Reduce = results =>
            from result in results
            group result by result.Category into g
            select new
            {
                Category = g.Key,
                Count = g.Sum(x => x.Count)
            };
    }
}
````  

There are some points to note here. The "counting" task is performed by
two expressions. The `Map` just gets data from the documents and the `Reduce`
performs the aggregation. This allows the RavenDB engine to
treat each step in isolation, that means it is not required to run them
sequentially and at the same time.

The output from the `Map` and `Reduce` steps needs to be the same. This
allows the engine to perform multiple reduce stages.

### Step 4: Initialize the `DocumentStore` and register the index on the server

Again, let's do it using our good friend pattern `DocumentStoreHolder`. At
this point, you probably know that this is the pattern to follow. Right?   

````csharp
using System;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace MapReduceIndexes
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

Again, we are asking the client API to find all indexes classes automatically and send them altogether to the server.
You can do that, using the `IndexCreation.CreateIndexes` method.

### Step 5: Consuming the index

Now we are ready to perform some queries.

````csharp
class Program
{
    static void Main(string[] args)
    {
        using (var session = DocumentStoreHolder.Store.OpenSession())
        {
            var results = session
                .Query<Products_ByCategory.Result, Products_ByCategory>()
                .Include(x => x.Category)
                .ToList();

            foreach (var result in results)
            {
                var category = session.Load<Category>(result.Category);
                Console.WriteLine($"{category.Name} has {result.Count} items.");
            }
        }
    }
}
````

This will list all the categories of the products. Remember that the `Include` function
ensures that all data is returned from the server in a single response.

## Exercise: Employee of the month

In Northwind, we have employees and orders. In this exercise you will
create an index that will select the "Employee of the Month", which
will be the employee with the most sales in a particular month.

This exercise picks up right where the previous one left off.

### Step 1: Write the model classes

Let's add two more model classes in your application.

````csharp
public class Order {
    public string Employee { get;  }
    public DateTime OrderedAt { get; }
}

public class Employee
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
````

### Step 2: Write the `Map-Reduce index definition`

````csharp
public class Employees_SalesPerMonth :
    AbstractIndexCreationTask<Order, Employees_SalesPerMonth.Result>
{
    public class Result
    {
        public string Employee { get; set; }
        public string Month { get; set; }
        public int TotalSales { get; set; }
    }

    public Employees_SalesPerMonth()
    {
        Map = orders =>
            from order in orders
            select new
            {
                order.Employee,
                Month = order.OrderedAt.ToString("yyyy-MM"),
                TotalSales = 1
            };

        Reduce = results =>
            from result in results
            group result by new
            {
                result.Employee,
                result.Month
            }
            into g
            select new
            {
                g.Key.Employee,
                g.Key.Month,
                TotalSales = g.Sum(x => x.TotalSales)
            };
    }
}
````
The difference here is that you are grouping by two fields. Nothing really special.

### Step 3: Consuming the index

Now we are ready to perform some queries.

````csharp
class Program
{
    static void Main(string[] args)
    {
        using (var session = DocumentStoreHolder.Store.OpenSession())
        {
            var query = session
                .Query<Employees_SalesPerMonth.Result, Employees_SalesPerMonth>()
                .Include(x => x.Employee);

            var results = (
                from result in query
                where result.Month == "1998-03"
                orderby result.TotalSales descending
                select result
                ).ToList();

            foreach (var result in results)
            {
                var employee = session.Load<Employee>(result.Employee);
                Console.WriteLine($"{employee.FirstName} {employee.LastName} made {result.TotalSales} sales.");
            }
        }
    }
}
````

Nice!

## Great job! Onto Lesson 5!

Awesome! You just learned the basics of Map-Reduce and how to use it with RavenDB.

**Let's move onto [Lesson 5](../lesson5/README.md) **
