# Unit 1, Lesson 7 - Storing, Modifying, and Deleting Documents

In this lesson, you will learn how to store, modify, and delete documents.

## Quick Start
Storing, modifying, and deleting documents is extremely easy with RavenDB.

Let me provide you a quick start demo.

````csharp
// storing a new document
string categoryId;
using (var session = DocumentStoreHolder.Store.OpenSession())
{
    var newCategory = new Category
    {
        Name = "My New Category",
        Description = "Description of the new category"
    };

    session.Store(newCategory);
    categoryId = newCategory.Id;
    session.SaveChanges();
}

// loading and modifying
using (var session = DocumentStoreHolder.Store.OpenSession())
{
    var storedCategory = session
        .Load<Category>(categoryId);

    storedCategory.Name = "abcd";

    session.SaveChanges();
}

// deleting
using (var session = DocumentStoreHolder.Store.OpenSession())
{
    session.Delete(categoryId);
    session.SaveChanges();
}
````

Any .NET object can be stored by RavenDB. It only needs to be serializable to
JSON.

The `Store` method is responsible to register the "storing" intention in the session.
You can access the document right after the `Store` call was made, even though the document
was not saved to the database yet. The `SaveChanges` method applies the registered
actions in the session to the database.

When you change the state of an entity, the session is smart enough to detect it and
update the matching document on the server side. The session keeps track of all the
entities you have loaded (with `Load` or `Query` methods), and when you call `SaveChanges`,
all changes to those entities are sent to the database in a *single remote call*.

The `Delete` method, which we have used in the last part of the code will delete the
matching document on the server side. You can provide the document ID or an
entity instance.

**All the changes are applied on the server side only after you call the `SaveChanges` method.**

> The session implements the Unit of Work pattern. Learn more reading the [official documentation](http://ravendb.net/docs/article-page/latest/csharp/client-api/session/what-is-a-session-and-how-does-it-work).

## Modifying Documents with the `Store` Method

Beyond saving a new entity, the `Store` method is also used to associate entities
of existing documents within the session. This is common in web applications. You
have one endpoint that sends the entity to the user, which modifies that entity and
then sends it back to your web application. You have a live entity instance, but
it is not loaded by the session or tracked by it. At that point, you have to
call the `Store` method on that entity, and because it doesn't have a null document
ID, it will be treated as an existing document and overwrite the previous version
on the database side.

````csharp
public class CategoryRepository
{
    // ...
    public void Update(Category category)
    {
        var (session = DocumentStoreHolder.Store.OpenSession())
        {
            session.Store(category);
            session.SaveChanges();
        }
    }
    // ...
}
````

`SaveChanges` should be called only once per session.

## Exercise: Creating a Basic Contacts CRUD

In this exercise we will create a `Console Application` to manage a basic
list of contacts.

### Step 1: Create a new project and install the latest `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
Contacts Manager. Then, in the `Package Manager Console`, issue the following
command:

```powershell
Install-Package RavenDB.Client -Version 5.4.5
```

This will install RavenDB.Client binaries, which you will need in order
to compile your code.

Then you will need to add the `using` namespace at the top of `Program.cs`:

````csharp
using Raven.Client;
using Raven.Client.Documents.Linq;
````

### Step 2: Initialize the `DocumentStore`

Let's do it using our good friend pattern `DocumentStoreHolder`. You learned about it in
[Lesson 4](../lesson4/README.md).

Note that, if the database specified in the `Database` property does not exist, the code below will create automatically one for you. The next time that you'll run your application again, the `ContactsManager` database won't re-created anymore and your application will able to resuse this one.

````csharp
using System;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace ContactsManager
{
    public static class DocumentStoreHolder
    {
        private static readonly Lazy<IDocumentStore> LazyStore =
            new Lazy<IDocumentStore>(() =>
            {
                var store = new DocumentStore
                {
                    Urls = new[] { "http://localhost:8080" },
                    Database = "ContactsManager"
                };

                store.Initialize();

                // Try to retrieve a record of this database
                var databaseRecord = store.Maintenance.Server.Send(new GetDatabaseRecordOperation(store.Database));

                if (databaseRecord != null)
                    return store;

                var createDatabaseOperation =
                    new CreateDatabaseOperation(new DatabaseRecord(store.Database));

                store.Maintenance.Server.Send(createDatabaseOperation);

                return store;
            });

        public static IDocumentStore Store =>
            LazyStore.Value;
    }
}
````

### Step 3: Instancing and running

Now that we have a `DocumentStoreHolder` correctly implemented, let's write
some application code.

````csharp
using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Documents;

namespace ContactsManager
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
        }
    }
}
````

I don't like the idea of having too many static methods.

### Step 4: Create the model class

In this exercise we will use a very simple model class.

````csharp
public class Contact
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
````
### Step 5: Implementing a basic options menu

Let's implement a basic console menu that permits the user to select
which operation should be executed.

````csharp
private void Run()
{
    while (true)
    {
        Console.WriteLine("Please, press:");
        Console.WriteLine("C - Create");
        Console.WriteLine("R - Retrieve");
        Console.WriteLine("U - Update");
        Console.WriteLine("D - Delete");
        Console.WriteLine("Q - Query all contacts (limit to 128 items)");

        var input = Console.ReadKey();

        Console.WriteLine("\n------------");

        switch (input.Key)
        {
            case ConsoleKey.C:
                CreateContact();
                break;
            case ConsoleKey.R:
                RetrieveContact();
                break;
            case ConsoleKey.U:
                UpdateContact();
                break;
            case ConsoleKey.D:
                DeleteContact();
                break;
            case ConsoleKey.Q:
                QueryAllContacts();
                break;
            default:
                return;
        }

        Console.WriteLine("------------");
    }
}
````
### Step 6: Implementing the logic to create a new contact

````csharp
private void CreateContact()
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        Console.WriteLine("Name: ");
        var name = Console.ReadLine();

        Console.WriteLine("Email: ");
        var email = Console.ReadLine();

        var contact = new Contact
        {
            Name = name,
            Email = email
        };

        session.Store(contact);

        Console.WriteLine($"New Contact ID {contact.Id}");

        session.SaveChanges();
    }
}
````

### Step 7: Retrieving information

````csharp
private void RetrieveContact()
{
    Console.WriteLine("Enter the contact id: ");
    var id = Console.ReadLine();
    var contactsReference = $"contacts/{id}-A";
	
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var contact = session.Load<Contact>(contactsReference);

        if (contact == null)
        {
            Console.WriteLine("Contact not found.");
            return;
        }

        Console.WriteLine($"Name: {contact.Name}");
        Console.WriteLine($"Email: {contact.Email}");
    }
}
````


### Step 8: Updating a contact

````csharp
private void UpdateContact()
{
    Console.WriteLine("Enter the contact id: ");
    var id = Console.ReadLine();
    var contactsReference = $"contacts/{id}-A";

    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var contact = session.Load<Contact>(contactsReference);

        if (contact == null)
        {
            Console.WriteLine("Contact not found.");
            return;
        }

        Console.WriteLine($"Actual name: {contact.Name}");
        Console.WriteLine("New name: ");
        contact.Name = Console.ReadLine();

        Console.WriteLine($"Actual email: {contact.Email}");
        Console.WriteLine("New email address: ");
        contact.Email = Console.ReadLine();

        session.SaveChanges();
    }
}
````

### Step 9: Deleting a contact

````csharp
private void DeleteContact()
{
    Console.WriteLine("Enter the contact id: ");
    var id = Console.ReadLine();
    var contactsReference = $"contacts/{id}-A";

    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var contact = session.Load<Contact>(contactsReference);

        if (contact == null)
        {
            Console.WriteLine("Contact not found.");
            return;
        }

        session.Delete(contact);
        session.SaveChanges();
    }
}
````

### Step 10: List all contacts

````csharp
private void QueryAllContacts()
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        var contacts = session.Query<Contact>().ToList();

        foreach (var contact in contacts)
        {
            Console.WriteLine($"{contact.Id} - {contact.Name} - {contact.Email}");
        }

        Console.WriteLine($"{contacts.Count} contacts found.");
    }
}
````

It is important to say that all contacts will be returned. 

## Transactions! Transactions!

In RavenDB all actions performed on documents are fully ACID (Atomicity,
Consistency, Isolation, and Durability). All these constraints are ensured
when you use a session and call the `SaveChanges` method. 

This is a great thing! 

Unlike many other NoSQL databases, RavenDB is an ACID
database. We were the first to become ACID over 10 years ago. 

More information is available on the [official documentation](https://ravendb.net/docs/article-page/4.0/csharp/client-api/faq/transaction-support)

## Great Job!

**Congratulations! You know the basics of RavenDB.**
