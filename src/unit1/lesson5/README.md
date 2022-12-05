# Unit 1, Lesson 5 - Loading Documents

We have been loading documents since [lesson 3](../lesson3/README.md), and now it's time to learn what alternatives you have.

## Before Loading, Establishing a `Session`

The session is the primary way your code interacts with RavenDB. You need
to create a session via the document store, and then use the session methods
to perform operations.

A session object is very cheap to create and very easy to use. To create a
session, you simply call the `DocumentStore.OpenSession()` method.

````csharp
using (var session = DocumentStoreHolder.Store.OpenSession())
{
    // ready to interact with the database
}
````

## The Basics of the `Load` Method

As the name implies, the `Load` method gives you the option of loading a document
or a set of documents, passing the document(s) ID(s) as parameters. The result will
be an object representing the document or `null` if the document does not exist.

A document is loaded only once in a session. Even though we call the `Load` method
twice passing the same document ID, only a single remote call to the server will
be made. Whenever a document is loaded, it is added to an internal dictionary managed
by the session.

### Exercise: Confirming that a document is loaded only once in a session

This exercise picks up right where the previous one in the last lesson left off.

Change your program to call the `Load` method two times passing the same
value as the parameter. Then, use the `Debug.Assert` method to confirm the
resulting objects are the same.

````csharp
using (var session = DocumentStoreHolder.Store.OpenSession())
{
    var p1 = session.Load<Product>("products/1-A");
    var p2 = session.Load<Product>("products/1-A");
    Debug.Assert(ReferenceEquals(p1, p2));
}
````


## Loading Multiple Documents with a Single `Load` Call

Load can also read more than a single document at a time. For example:

````csharp
var products = session.Load<Product>(new [] {
    "products/1-A",
    "products/2-A",
    "products/3-A"
});
````

This will result in a dictionary with three documents in it, **retrieved in a single
remote call from the server**. 

## Loading Related Documents in a Single Remote Call

The easiest way to kill your application performance is to make a lot of remote calls. RavenDB provides a lot of features to help you
significantly reduce calls and boost performance.

Consider the Northwind `products/1-A` document:

````csharp
{
    "Name": "Chai",
    "Supplier": "suppliers/1-A",
    "Category": "categories/1-A",
    "QuantityPerUnit": "10 boxes x 20 bags",
    "PricePerUnit": 18,
    "UnitsInStock": 1,
    "UnitsOnOrder": 39,
    "Discontinued": false,
    "ReorderLevel": 10,
    "@metadata": {
        "@collection": "Products"
    }
}
````

As you can see, the `Supplier` and `Category` properties are clearly references to
other documents.

Considering you need to load the product and the related category, how would you
write the code? Your first attempt could be something like this:

````csharp
var p = session.Load<Product>("products/1-A");
var c = session.Load<Category>(p.Category);
````

This approach will make two remote calls -- not good.

````csharp
var p = session
    .Include<Product>(x => x.Category)
    .Load("products/1-A");

var c = session.Load<Category>(p.Category);
````

The `Include` session method changes the way RavenDB will process the request.

It will:

* Find a document with the ID: `products/1-A`
* Read its `Category` property value
* Find a document with that ID
* Send both documents back to the client

When the `session.Load<Category>(p.Category);` is executed, the document is in the
session cache and no additional remote call is made.

Here is a powerful example of the application of this technique in a complex scenario.

````csharp
var order = session
    .Include<Order>(x => x.Company)
    .Include(x => x.Employee)
    .Include(x => x.Lines.Select(l => l.Product))
    .Load("orders/1-A");
````

This code will, in a single remote call, load the order, include the company
and employee documents, and also load *all* the products in all the lines
in the order.

## Exercise: Exploring Orders

In this exercise we will create an "Orders Explorer" for the Northwind database.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

As you learned in lesson 2, start Visual Studio and create a new `Console Application Project` named
Northwind. Then, in the `Package Manager Console`, issue the following command:

```powershell
Install-Package RavenDB.Client -Version 5.4.5
```

Then you will need to add the `using` namespace at the top of `Program.cs`:

````csharp
using Raven.Client.Documents;
````

### Step 2: Create the `DocumentStoreHolder`

As you learned in lesson 3, add a new class in your project named `DocumentStoreHolder`.
Then replace the file content with:

````csharp
using System;
using Raven.Client;
using Raven.Client.Documents;

namespace OrdersExplorer
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

                return store.Initialize();
            });

        public static IDocumentStore Store =>
            LazyStore.Value;
    }
}
````

### Step 3: Add Northwind model classes to your project
As you learned in lesson 2, you can work only with dynamic objects. It is a good idea to have compiler/IDE support in your projects.

Add a new class in your project named NorthwindModels. Then replace the file content with [this](NorthwindModels.cs).

### Step 4: Request an order number

Back to `Program.cs`, let's create a minimal user interface which requests order numbers.

````csharp
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
                WriteLine("Please, enter an order # (0 to exit): ");

                int orderNumber;
                if (!int.TryParse(ReadLine(), out orderNumber))
                {
                    WriteLine("Order # is invalid.");
                    continue;
                }

                if (orderNumber == 0) break;

                PrintOrder(orderNumber);
            }

            WriteLine("Goodbye!");
        }

        private static void PrintOrder(int orderNumber)
        {
        }
    }
}
````

### Step 5: Print order number, company, employee, and line products

Now it's time to load order data, but only if there is an order with the specified order number.

````csharp
private static void PrintOrder(int orderNumber)
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var order = session
            .Include<Order>(o => o.Company)
            .Include(o => o.Employee)
            .Include(o => o.Lines.Select(l => l.Product))
            .Load($"orders/{orderNumber}-A");

        if (order == null)
        {
            WriteLine($"Order #{orderNumber} not found.");
            return;
        }

        WriteLine($"Order #{orderNumber}");

        var c = session.Load<Company>(order.Company);
        WriteLine($"Company : {c.Id} - {c.Name}");

        var e = session.Load<Employee>(order.Employee);
        WriteLine($"Employee: {e.Id} - {e.LastName}, {e.FirstName}");

        foreach (var orderLine in order.Lines)
        {
            var p = session.Load<Product>(orderLine.Product);
            WriteLine($"   - {orderLine.ProductName}," +
                      $" {orderLine.Quantity} x {p.QuantityPerUnit}");
        }
    }
}
````

## Great Job! 

**Let's move on to [Lesson 6](../lesson6/README.md) and learn about querying, now in C#.**
