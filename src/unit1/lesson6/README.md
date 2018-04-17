# Unit 1, Lesson 6 - Querying Fundamentals in C# 

Querying is a large part of what a database does, and RavenDB doesn't disappoint.

As you learned in the lesson 2, querying with RavenDB is really easy. In this lesson, you will learn the fundamentals of querying using RavenDB in C#.

## LINQ Support via `Query` Session Method

RavenDB takes full advantage of LINQ support in C#. This allows you to
express very natural queries on top of RavenDB in a strongly typed and safe
manner.

Queries allow you to load documents that match a particular predicate.

You get access to LINQ support via the `Query` method from the session object.

Like documents loaded via the `Load` call, documents that were loaded via
`Query` are managed by the session (unless you are doing a projection).

Queries in RavenDB don't behave like queries in relational databases. RavenDB
does not allow computation during queries, and it doesn't have problems with
table scans because all queries are indexed (even if you didn't create any indexes).

## Exercise: Querying orders of a company

This time, you will write an application which requests a company Id. Then
you will list the orders made by this company.

I think you got the basics from the previous exercises. So I will not repeat details that you already know.

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
using Raven.Client.Documents;
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

                if (!int.TryParse(ReadLine(), out var companyId))
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

With an id, you can now query the orders.

````csharp
private static void QueryCompanyOrders(int companyId)
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var orders = (
            from order in session.Query<Order>()
                                    .Include(o => o.Company)
            where order.Company == $"companies/{companyId}-A"
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
            WriteLine($"{order.Id} - {order.OrderedAt}");
        }
    }
}
````

## Why Not Use RQL?

Using LINQ is natural for C# developers. But what if you want to discover the power of RQL from C#? 

```csharp
private static void QueryCompanyOrders(int companyId)
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var orders = session.Advanced.RawQuery<Order>(
            "from Orders " +
            $"where Company=='companies/{companyId}-A'" +
            "include Company"
        );

        var company = session.Load<Company>($"companies/{companyId}-A");

        if (company == null)
        {
            WriteLine("Company not found.");
            return;
        }

        WriteLine($"Orders for {company.Name}");

        foreach (var order in orders)
        {
            WriteLine($"{order.Id} - {order.OrderedAt}");
        }
    }
}
``` 

Did I say that I love RQL?!

## Great job! 

We will discuss a lot about querying when we start to talk about RavenDB indexes.

**Let's move on to [Lesson 7](../lesson7/README.md) and learn how to create, change and delete documents.**
