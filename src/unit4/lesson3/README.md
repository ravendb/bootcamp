# Unit 4, Lesson 3 - Considerations on the client side

In this lesson, we will spend some time exploring the interaction between clients and distributed
databases.

## Coming back to the DocumentStore
When creating an instance of the DocumentStore, you should specify the list of nodes in the cluster that you are connecting on.

```csharp
var store = new DocumentStore
{
    Urls =
    {
        "http://127.0.0.1:8080" ,"http://127.0.0.2:8080" ,
        "http://128.0.0.3:8080" 
    },
    Database = "Northwind"
};
store.Initialize ();
```

Please note that the fact the database you are trying to access does not have instances on all cluster nodes is not important.

As we mentioned earlier, all cluster nodes contain the full topology for all the databases hosted in the cluster. The very first thing that a client will do upon initialization is to query the defined URLs and figure out what the actual nodes are that it needs to get to the database you are trying to connect.


## Why to list all the nodes in the cluster

To be honest, it is not necessary.

By listing all the nodes in the cluster, we can ensure that if a single node is down and we bring a new client up, we’ll still be able to get the initial topology. If the cluster sizes are small (three to five), you will typically list all the nodes in the cluster. But for more massive clusters, you will usually just list enough nodes that are having them all go down at once will mean that you have more pressing concerns than a new client coming up.

The main idea is to list nodes that you assume will be live when the client try to connect.

For extra reliability, the client will also cache the topology on disk, so even if the document store were initialized with a single node that was down at the time the client was restarted, the client would still remember where to look for our database. It’s only an entirely new client that needs to have the full listing. But it’s good practice to list at least a few nodes, just in case.

##  Exploratory challenge: Testing the client

Now it is time to you to verify that all I am saying is true. For this:

1. Create a client program (as you already know from the previous units) for some replicated database.
2. When configuring the `DocumentStore` object, specify the address of a cluster node that does not contains an instance of the database
3. Try to do a query.


## Dealing with failures

Remember RavenDB is safe by default!

While it may seem that an alternate failing (the client isn’t even going to notice) or the preferred node failing (cluster will demote, clients will automatically switch to the first alternate) is all that we need to worry about, those are just the most straightforward and most obvious failure modes that you need to handle in a distributed environment.

More exciting cases include a node that was split off from the rest of the cluster, along with some (but not all) of the clients. In that case, different clients have very different views about who they can talk to. That’s why each client can failover independently of the cluster. By having the database topology, they know about all the database instances and will try each in turn until they’re able to find an available server that can respond to them.

This behavior is entirely transparent to your code, and an error will be raised only if we can’t reach any of the database instances. 

## How database group works saving data

Whenever a write is made to any of the database instances, it will disseminate that write to all the other instances in the group. That happens in the background and is continuously running. Most of the time, you don’t need to think about it. You write the data to RavenDB, and it shows up in all the nodes on its own.

## Dealing with important data

Sometimes, it is not enough to ensure that you wrote that value to a single node (and made sure it hit the disk). You need to be sure that this value resides in more than one machine. You can do that using write assurance, which is available using the `WaitForReplicationAfterSaveChanges` method. 

```csharp
using (var session = store.OpenSession ())
{
    var newprospect = new Prospect
    {
        Name = "John Doe",
        Company = "ABC Tech",
        Email = "johndoe@abctech.com"
    };
    session.Store(newprospect);
    session.Advanced.WaitForReplicationAfterSaveChanges(replicas: 1);
    session.SaveChanges ();
}
```

There isn’t much to change when you move from a single node to a cluster. But here we are asking the database instance we wrote to not to confirm that write until it has been replicated at least once.

## Great job! That's all.

Wow! You made it! At this point you already know the basic (and not so basic) aspects of RavenDB. Congratulations! Again, you can learn more using the [documentation](http://ravendb.net/docs) available online.

Another valuable resource is the [Ayende's book](https://github.com/ravendb/book) (which is a definitive guide and it is free). 

Thanks for using this bootcamp to learn RavenDB.
