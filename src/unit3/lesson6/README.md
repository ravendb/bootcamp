# Unit 3, Lesson 6 - Data Subscriptions - I Would Like to be Notified when an Important Event Happens

You already know the basics about how to use RavenDB and you are ready to move on. Please
note that there is an [extensive documentation](http://ravendb.net/docs) available online. You can learn more about other features like clustering, for example. There is much more to learn than we covered.

In this last lesson, you will learn how to work with Data Subscriptions.

## Data Subscriptions

It's simpler to explain this concept with an example. Consider the following query:

```sql
from Orders 
where Lines.length > 5
```

This would retrieve all the big orders from your database. But what if a new big order 
is added after you run this query. What if you want to be notified whenever a big order
occurs? 

## Exercise: Creating a Data Subscription

Open the Studio and the Northwind database.

To create a Data Subscription you need to open the `Settings` section of your database.
Click on the `Manage Ongoing Tasks` option, then click on the `Add Task` button.

![Subscription task](media/new_subscription_task.png)

Now, click on `Subscription`.

RavenDB is asking you for a Task Name (let's call it `Big Orders`) and let's provide the very
same query we used before.

![Big orders](media/big_orders.png)

The user interface allows you to do a test before saving. 

Save it!

## Exercise: Subscribing the Data Subscription

In this exercise, you will learn how to subscribe and consume a 
data subscription.

>Data subscriptions are consumed by clients, called subscription workers. 
In any given moment, **only one worker can be connected to a data subscription**. 
A worker connected to a data subscription receives a batch of documents and gets to process it. 
When it's done (depends on the code that the client gave the worker, can take from seconds to hours), 
it informs the server about the progress and the server is ready to send the next batch.

### Step 1: Create a new project and install the `RavenDB.Client` package

Start Visual Studio and create a new `Console Application Project` named
`BatchOperationsWithRavenDB`. Then, in the `Package Manager Console`, issue the following
command:

```powershell
Install-Package RavenDB.Client -Version 5.4.5
```

### Step 2: Initialize the `DocumentStore`

Let's manage the `DocumentStore` using our great friend the `DocumentStoreHolder` pattern.  

````csharp
using System;
using Raven.Client;
using Raven.Client.Documents;

namespace BatchOperationsWithRavenDB
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

### Step 3: Subscribing...

Subscriptions are consumed by processing batches of documents received from the server. 
We need to get an instance of `SubscriptionWorker`.

Add `using System.Threading.Tasks;` and `using static System.Console;` at the top of `Program.cs`, then write the `Main` method:

```csharp
static void Main(string[] args)
{
    var subscriptionWorker = DocumentStoreHolder.Store
        .Subscriptions
        .GetSubscriptionWorker<Order>("Big Orders");

    // Called when the worker loses its connection and is about to reconnect.
    subscriptionWorker.OnSubscriptionConnectionRetry += exception =>
        WriteLine($"Connection lost, retrying: {exception.Message}");

    var subscriptionRuntimeTask = subscriptionWorker.Run(batch =>
    {
        foreach (var order in batch.Items)
        {
            // business logic here.
            WriteLine(order.Id);
        }
    });

    // Run() returns a Task. If the subscription stops with an error
    // (e.g. it was deleted, or the server can't be reached), the error is
    // stored in that Task, so report it instead of losing it silently.
    subscriptionRuntimeTask.ContinueWith(
        task => WriteLine($"Subscription stopped: {task.Exception.GetBaseException().Message}"),
        TaskContinuationOptions.OnlyOnFaulted);

    WriteLine("Press any key to exit...");
    ReadKey();

    subscriptionWorker.Dispose();
}
```

## How Data Subscriptions Work

There are some important facts that you need to know to use this feature correctly.

* Documents that match a pre-defined criteria are sent in batches from the server to the client.
* The client sends an acknowledgment to the server once it is done with processing the batch.
* The server keeps track of the latest document that was acknowledged by the client, so that processing can be continued from the latest acknowledged position if it was paused or interrupted.

## Great job! Next stop: Clusters!

Congratulations! Now you know some advanced features of RavenDB. In the next Unit we will start to work with clusters.
