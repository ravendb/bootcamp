# Unit 4, Lesson 2 - Database and Database Group

Now that you already know how to set up a cluster, let's learn some in depth concepts about how it works. Let's also learn how to manage a database in the cluster.

## It's all about "Operations"

Operations in RavenDB can be classified into two categories:

1) Cluster-Wide Operations: They impact the entire cluster, like creating a new database.

2) Internal Database Operations: They impact a single database, like creating a new document.

This distinction is vital because cluster-wide operations demand consensus between the nodes of the cluster and it is only possible when the majority of the nodes is working.  RavenDB uses a consensus protocol called RAFT. To create a database in a cluster with three nodes, it would be necessary that at least two nodes are working (the majority). This is the reason why you should not have a cluster with only two nodes. When one is down, there is no majority.

> There are a lot of details about how the RAFT consensus protocol works in RavenDB that are beyond the scope of this bootcamp. If you want to understand it in-depth, I recommend you to read [Ayende's book](https://github.com/ravendb/book) (which is the definitive reference about RavenDB by the CEO, and it is free!).  

Database operations, on the other hand, are treated quite differently. Each node in the cluster has the full copy of the topology which specifies which nodes host which databases. The connection between the database instances does not use the consensus protocol. Instead of this, they're direct connections among the various nodes, and they form a multi-master mesh. A write to any of those nodes will be automatically replicated to all the other nodes.

Cluster Consensus provides firm consistency. Unfortunately, we also need to ensure availability of the databases, and it is a tradeoff (look for the [CAP theorem](https://en.wikipedia.org/wiki/CAP_theorem) to understand the reasons). We decided to provide consistency where it is indispensable and chose explicitly for availability elsewhere.

## Exploratory Challenge: What Happens when a Node Goes Down?

Assuming you are running a cluster with three nodes, this is what you see in the `Management Studio` under `Manage Server` → `Cluster`.

![cluster status](./media/status_cluster.png)

I recommend you stop some nodes now and see what happens in the `Cluster Topology` diagram on the right side.

## Focus on Robustness

RavenDB's design is mainly focused on robustness. Once the cluster is set up, each database instance on the various nodes is independent. It can accept writes without consulting the other instances of the same database.

Even if the majority of the cluster is down, as long as a single node containing a database instance is available, RavenDB can process reads and writes. If a user is writing to the database, RavenDB will hold the data. 

## Creating a New Database with Replication

Whenever you create a new database you can specify a replication factor. That is the number of the cluster nodes that will contain an instance of the database.

In the `New database` dialog, enter the name and click on `Next` (not `Quick Create`) to get to the `Replication & Sharding` step, where you can set the `Replication Factor`.

![new database](media/new_database.png)

It is important to remember that creating a database is a cluster-wide operation and requires a majority to work.

## Exploratory Challenge: Creating a Distributed Database

To practice, create a new database with the replication factor set to two. Let's call it `MyDatabase`. (I am assuming that you are running the cluster we set up in the last lesson)

## Managing the Database Group

As you already know, the database group is the set of cluster nodes containing the instances of a database. 

You can manage the database group of a specified database accessing the `Databases` option in the left panel.

![new database](media/manage_group.png)

The image shows that the database we just created has instances on two nodes, Node B and Node C, but it can be different on your computer!

The screenshot was taken on Node A, which has no instance of `MyDatabase`. The database is still shown as `Online`, because it is running in the cluster. The filters at the top tell you where the instances are: `Local (Node A)` counts the databases that have an instance on the node you are connected to, and `Remote` counts the ones that only exist on other nodes.

Clicking on the `Manage Group` button of the database that you want to manage allows you to change the nodes where there are instances of the selected database.

It is important to remember that managing a database group is a cluster-wide operation that requires a majority to work.

##  Exploratory Challenge: Managing the Database Group

Let's try to add a new cluster node to the Database Topology. After this, please, access the `Management Studio` of the corresponding node and see how amazing RavenDB is at replicating your data.

## Great Job! 

**Let's move onto [Lesson 3](../lesson3/README.md).**