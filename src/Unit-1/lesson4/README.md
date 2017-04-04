# Unit 1, Lesson 4 - Loading documents

You have been loading documents since [lesson 2](../lesson2/README.md), and now it is time to learn
what alternatives you have.

## Before loading, establishing a `Session`
The session is the primary way your code interacts with RavenDB. You need
to create a session, via the document store, and then use the session methods
to perform operations.

A session object is very cheap to create and very easy to use. To create a
session, you simple call `DocumentStore.OpenSession()` method.

````csharp
using (var session = DocumentStoreHolder.Store.OpenSession())
{
    // ready to interact with the database
}
````

## The basics of the `Load` method
As the name implies, the `Load` method gives you the option of loading a document
or a set of documents, passing the document(s) id(s) as parameters. The result will
be an object representing the document or `null` if the document does not exists.

A document is loaded only once in a session. Even though we call the `Load` method
twice passing the same document id, only a single remote call to the server will
be made. Whenever a document is loaded, it is added to an internal dictionary managed
by the session.

### Exercise: Confirming that a document is loaded only once in a session

This exercise picks up right where the previous one, in the previous lesson, left off.

Change your program to call the `Load` method two times passing the same
value as the parameter. Then, use the `Debug.Assert` method to confirm the
resulting objects are the same.

````csharp
using (var session = DocumentStoreHolder.Store.OpenSession())
{
    var p1 = session.Load<Product>("products/1");
    var p2 = session.Load<Product>("products/1");
    Debug.Assert(ReferenceEquals(p1, p2));
}
````

## Loading documents passing a simpler id
RavenDB ids are usually in form of `<name-of-collection>/<number>` (like `products/1`, `categories/7`, and so on).
This makes it very easy to look at and debug. But sometimes, it would be easier to
specify only the number.

````csharp
Product product = session.Load<Product>(1);
````

This code is really nice to use, especially in web scenarios. It would work because
the default RavenDB conventions (remember from [lesson 3](../lesson3#introducing-conventions)?) infers the collection
name from the type parameter name.


## Loading multiple documents with a single `Load` call
Load can also read more than a single document at a time. For example:

````csharp
Product[] products = session.Load<Product>(new [] {
    "products/1",
    "products/2",
    "products/3"
});
````

Because of the "collection inference", you can specify a simpler id.

````csharp
Product[] products = session.Load<Product>(1, 2, 3);
````

This will result in an array with three documents in it, **retrieved in a single
remote call from the server**. The order in the array matches the order
of the ids passed to the `Load` call.

You can even load documents belonging to multiple types in a single call, like so:

````csharp
object[] items = session.Load<object>("products/1", "categories/2");

Product p = (Product) items[0];
Category c = (Category) items[1];
````

## Exercise: Loading multiple documents with a single `Load` call

Now that you know how to load multiple documents with a single call, you can try it.

I strongly recommend that you get some additional model classes using the
`Generate class` tool while editing documents in the RavenDB Studio. Then
try to load some documents of these types.

## Loading related documents in a single remote call

As you probably know, the easiest way to kill your application performance is
to make a lot of remote calls. RavenDB provides a lot of features to help you
mitigate that problem.

Consider the Northwind `products/1` document:

````csharp
{
    "Name": "Chai",
    "Supplier": "suppliers/1",
    "Category": "categories/1",
    "QuantityPerUnit": "10 boxes x 20 bags",
    "PricePerUnit": 18,
    "UnitsInStock": 39,
    "UnitsOnOrder": 0,
    "Discontinued": false,
    "ReorderLevel": 10
}
````

As you can see, the `Supplier` and `Category` properties are clearly references to
other documents.

Considering you need to load the product and the related category, how would you
write the code? Your first attempt could be something like that:

````csharp
var p = session.Load<Product>(1);
var c = session.Load<Category>(p.Category);
````

This approach will make two remote calls and this is not a good thing.
But, don't worry.

````csharp
var p = session
    .Include<Product>(x => x.Category)
    .Load(1);

var c = session.Load<Category>(p.Category);
````

The `Include` session method changes the way RavenDB will process the request.
Basically, it will:

* Find a document with the key: `products/1`
* Read its `Category` property value.
* Find a document with that key.
* Send both documents back to the client.

When the `session.Load<Category>(p.Category);` is executed, the document is in the
session cache and no additional remote call is made.

Here is a powerful example of application of this technique in a complex scenario.

````csharp
var order = session
    .Include<Order>(x => x.Company)
    .Include(x => x.Employee)
    .Include(x => x.Lines.Select(l => l.Product))
    .Load("orders/1");
````

This code will, in a single remote call, load the order, include the company
and employee documents, and also load *all* the products in all the lines
in the order.

## Exercise: Exploring Orders

In this exercise you will create an "Orders Explorer" for the Northwind database.

### Step 1: Create a new project and install the latest `RavenDB.Client` package
As you learned in lesson 2, start Visual Studio and create a new `Console Application Project` named
Northwind. Then, in the `Package Manager Console`, issue the following command:

```Install-Package RavenDB.Client```

Then you will need to add the `using` namespace at the top of `Program.cs`:

````csharp
using Raven.Client.Document;
````

### Step 2: Create the `DocumentStoreHolder`

As you learned in lesson 3, add a new class in your project named `DocumentStoreHolder`.
Then replace the file content with:

````csharp
using System;
using Raven.Client;
using Raven.Client.Document;

namespace OrdersExplorer
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

                return store.Initialize();
            });

        public static IDocumentStore Store =>
            LazyStore.Value;
    }
}
````

### Step 3: Add Northwind model classes to your project
As you learned in lesson 2, you could work only with dynamic objects. Anyway,
it is a good idea to have compiler/IDE support in your projects.

Add a new class in your project named NorthwindModels. Then replace the file
content with [this](NorthwindModels.cs).

### Step 4: Request an order number
Back to `Program.cs`, let's create a minimal user interface which requests
an order numbers.

````csharp
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

Now, it's time to load order data, but only if there is an order with the
specified order number.

````csharp
private static void PrintOrder(int orderNumber)
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var order = session
            .Include<Order>(o => o.Company)
            .Include(o => o.Employee)
            .Include(o => o.Lines.Select(l => l.Product))
            .Load(orderNumber);

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

## Great job! Onto Lesson 5!

Awesome! This *long* lesson is done and you know a lot about how to load documents from a RavenDB database.

**Let's move onto [Lesson 5](../lesson5/README.md) and learn about querying.**
