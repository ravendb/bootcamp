# Unit 1, Lesson 5 - Querying fundamentals

Querying is a large part of what a database does, and RavenDB doesn't disappoint
in this matter.

In this lesson, you will learn the fundamentals of querying using RavenDB.

## LINQ support via `Query` session method

RavenDB is taking full advantage of LINQ support in C#. This allows you to
express very natural queries on top of RavenDB in a strongly typed and safe
manner.

Queries allow you to load documents that match a particular predicate.

You get access to LINQ support via `Query` method from the session object.

Like documents loaded via the `Load` call, documents that were loaded via
`Query` are managed by the session.

Queries in RavenDB don't behave like queries in relational databases. RavenDB
does not allow computation during queries, and it doesn't have problems with
table scans because all queries are indexed (even if you don't create any indexes).

## Exercise: Querying orders of a company

This time, you will write an application which requests a company Id. Then
you will list the orders made by this company.

I think you got the basics from the previous exercises. So, I will not repeat
myself providing you details that you already know.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

### Step 2: Create the `DocumentStoreHolder`

### Step 3: Add Northwind model classes to your project

### Step 4: Request a company Id

Back to `Program.cs`, let's create a minimal user interface which requests a
company Id.

````csharp
using System;
using System.Linq;
using static System.Console;
using Raven.Client.Document;
using NorthwindModels;

namespace OrdersExplorer
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                WriteLine("Please, enter a company id (0 to exit): ");

                int companyId;
                if (!int.TryParse(ReadLine(), out companyId))
                {
                    WriteLine("Order # is invalid.");
                    continue;
                }

                if (companyId == 0) break;

                QueryCompanyOrders(companyId);
            }

            WriteLine("Goodbye!");
        }

        private static void QueryCompanyOrders(int companyId)
        {
        }
    }
}
````

### Step 5: Query the orders for the specified company

With an id, it's time to query the orders.

````csharp
private static void QueryCompanyOrders(int companyId)
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var orders = (
            from order in session.Query<Order>()
                                    .Include(o => o.Company)
            where order.Company == $"companies/{companyId}"
            select order
            ).ToList();

        var company = session.Load<Company>(companyId);

        if (company == null)
        {
            WriteLine("Company not found.");
            return;
        }

        WriteLine($"Orders for {company.Name}");

        foreach (var order in orders)
        {
            WriteLine($"  {order.Id} - {order.OrderedAt}");
        }
    }
}
````
## Safe by default

The default value of a page size for a query is 128 results. In order to retrieve a different number of results in a single query use `.Take(pageSize)` method.

> You can learn more about paging reading the [official documentation](http://ravendb.net/docs/article-page/latest/csharp/indexes/querying/paging)

## Great job! Onto Lesson 6!

Awesome! This was a short lesson. We will discuss a lot about querying when we
start to talk about RavenDB indexes.

**Let's move onto [Lesson 6](../lesson6/README.md) and learn how to create, change and delete documents.**
