# Unit 2, Lesson 6 - Getting started with Transformers

Hello again!

It's time to learn about an important RavenDB concept: result transformers.

## What are transformers?

Transformers are server side transformations that allow you to project
specific data to the client! This allows you to save some bandwidth.

## Exercise: Getting started with transformers

To learn a new concept, there's nothing better than doing some little experiment. Right?
Let's do it.

In this exercise we will load a `Company` document from the server and then
write the company name on the console. First we will do it using what you already
know. Then, let's change the code to use transformers.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`LearningTransformers`. Then, in the `Package Manager Console`, issue the following
command:

```Install-Package RavenDB.Client```

This will install the latest RavenDB.Client binaries, which you will need in order
to compile your code.

### Step 2: Write the model classes

As usual, when just loading documents, we just need to create the model with
the properties we will use.

````csharp
public class Company
{
    public string Name { get; set; }
}
````

### Step 3: Initialize the `DocumentStore`

Here we go again. let's manage the `DocumentStore` using the `DocumentStoreHolder` pattern.  

````csharp
using System;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace LearningTransformers
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

                return store;
            });

        public static IDocumentStore Store =>
            LazyStore.Value;
    }
}
````

Remember to have started the RavenDB server.

### Step 4: Loading the company information

Now, it's time to load a company document from the server.

````csharp
class Program
{
    static void Main(string[] args)
    {
        using (var session = DocumentStoreHolder.Store.OpenSession())
        {
            var company = session.Load<Company>("companies/89");
            Console.WriteLine(company.Name);
        }
    }
}
````

Easy. Right?

There is nothing wrong here. But looking closer what happened you will see that
the client API sent the following request to the server.

````
http://localhost:8080/databases/Northwind/docs?id=companies%2F89
````

Which produced the following response:

````json
{
    "ExternalId": "WHITC",
    "Name": "White Clover Markets",
    "Contact": {
        "Name": "Karl Jablonski",
        "Title": "Owner"
    },
    "Address": {
        "Line1": "305 - 14th Ave. S. Suite 3B",
        "Line2": null,
        "City": "Seattle",
        "Region": "WA",
        "PostalCode": "98128",
        "Country": "USA"
    },
    "Phone": "(206) 555-4112",
    "Fax": "(206) 555-4115"
}
````

A lot of information if you just need the `Name` property.

### Step 5: Writing your first transformer

Transformers are LINQ-based server-side projection functions and nothing more.

Similarly to indexes, you create transformers definitions as a class.

````csharp
public class Company_JustName : AbstractTransformerCreationTask<Company>
{
    public Company_JustName()
    {
        TransformResults = companies =>
            from company in companies
            select new {company.Name};
    }
}
````

Pretty awesome!

### Step 6: Using transformers

Now you wrote your first transformer, it's time to use it.

````csharp
static void Main(string[] args)
{
    new Company_JustName().Execute(DocumentStoreHolder.Store);
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var company = session.Load<Company_JustName, Company>("companies/89");
        Console.WriteLine(company.Name);
    }
}
````

Because transformers are server-side artifacts, you need to create them on the
server before they can be used. You can do it calling the `Execute` method (as in the
sample code) or using the `IndexCreation.CreateIndexes` function.

Now, looking closer at what happened, you will see that the client API sent the following request to the server:

````
http://localhost:8080/databases/Northwind/queries/?&transformer=Company%2FJustName&id=companies%2F89
````

And this was the response.

````JSON
{
    "Results": [
    {
        "$values": [
            {
                "Name": "White Clover Markets"
            }
        ],
        "@metadata": {
            "Last-Modified:" "2016-08-05T14:02:42.7387677Z",
            "Raven-Last-Modified": "2016-08-05T14:02:42.7387677",
            "@etag": "3AD2616E-A033-8B3B-BE04-BE84ED03B469"
        }
    }
    ],
    "Includes": [ ]
}
````

Yes! There is some additional information here (that will be used to prevent
future requests with identical responses from the server). But, only the properties
you need are present.

> Because you are working with projections, and not directly with documents, they are not tracked by the session and won't be saved in the database if you call `SaveChanges`.

## Using transformers to load information from multiple documents

Now you know that transformers are pretty cool. Right? One very important
feature is the ability to reference other documents.

Let's learn how to do it.

## Exercise: Referencing other documents

In this exercise you will learn how to reference other documents using
transformers. You will write a program to load product information combined with
the referenced supplier.  

This exercise picks up right where the previous one left off.

### Step 1:  Write the model classes
What we want to do is to learn the product name and the supplier name. So
this is the model we need:

````csharp
public class Product
{
    public string Name { get; set; }
    public string Supplier { get; set; }
}

public class Supplier
{
    public string Name { get; set; }
}
````

### Step 2: Writing the transformer

Having the model classes, let's write the transformer.

````csharp
public class Products_ProductAndSupplierName : AbstractTransformerCreationTask<Product>
{
    public class Result
    {
        public string ProductName { get; set; }
        public string SupplierName { get; set; }
    }

    public Products_ProductAndSupplierName()
    {
        TransformResults = products =>
            from product in products
            let category = LoadDocument<Supplier>(product.Supplier)
            select new
            {
                ProductName = product.Name,
                SupplierName = category.Name
            };
    }
}
````

Here, you used the powerful `LoadDocument` server-side function to load
the related document. Similarly to index definitions, we defined a projection class named `Result`.

### Using the transformer

Having the transformer definition, it's time to send it to the server and consume it.

````csharp
class Program
{
    static void Main(string[] args)
    {
        new Products_ProductAndSupplierName().Execute(DocumentStoreHolder.Store);
        using (var session = DocumentStoreHolder.Store.OpenSession())
        {
            var product = session.Load<
                Products_ProductAndSupplierName,
                Products_ProductAndSupplierName.Result
                >("products/1");

            Console.WriteLine($"{product.ProductName} from {product.SupplierName}");
        }
    }
}
````

No more multiple `Load` calls to get data.

## Great job! Onto Lesson 7!

Awesome! You just learned how to save a lot of bandwidth with transformers.

**Let's move onto [Lesson 7](../lesson7/README.md) **
