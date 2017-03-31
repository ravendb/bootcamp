# Unit 2, Lesson 7 - Statistics and some words about stale indexes!

At this point, you know a lot about storing, loading and querying documents using
RavenDB, and that is awesome. Right?

In this lesson you will learn how to get details of a just executed query.  

## Introducing `RavenQueryStatistics`

Every time you perform a query using the client API, you can request
some additional info using the `Statistics` method.

````csharp
RavenQueryStatistics stats;

var orders = (
    from order in session.Query<Order>().Statistics(out stats)
    where order.Company == "companies/1"
    orderby order.OrderedAt
    select order
    ).ToList();

Console.WriteLine($"Index used was: {stats.IndexName}");
````

The results are available via a [`RavenQueryStatistics` object](https://ravendb.net/docs/article-page/latest/all/glossary/raven-query-statistics).
Among the details you can get from the query statistics, you have:

* Whether the index was stale or not.
* The duration of the query on the server side.
* The total number of results (regardless of paging).
* The name of the index that this query ran against.
* The last document etag indexed by the index.
* The timestamp of the last document indexed by the index.

## Stale index?

This is a very important fact: **RavenDB queries are BASE. Reads and writes by document ID are always ACID**.

RavenDB performs indexing in a background thread, which is executed whenever new data comes in
or existing data is updated. This allows the server to respond quickly, even when large
amounts of data have been changed. However, in that case you may query stale indexes and, because
of that, some query results may not be fully up to date. Usually, the time between a document being
updated and the relevant indexes being updated is measured in milliseconds.

### How to know if an index is stale?

RavenDB is honest and clear about stale indexes. It is really easy to know if a query used a
stale index just checking the statistics.

````csharp
RavenQueryStatistics stats;

var orders = (
    from order in session.Query<Order>().Statistics(out stats)
    where order.Company == "companies/1"
    orderby order.OrderedAt
    select order
    ).ToList();

if (stats.IsStale)
{
    // ..
}
````

The `IsStale` property will be `true` whenever the index used to perform the query is not
up to date. In this sample, probably an Order was added or changed, and the indexes didn't
have enough time to fully update before the query.

### Forcing non-stale results

If you need to make sure that your results are up to date, then you can use the `Customize` method.

````csharp
RavenQueryStatistics stats;

var query = session.Query<Order>()
    .Customize(q => q.WaitForNonStaleResultsAsOfNow(TimeSpan.FromSeconds(5)));

var orders = (
    from order in query
    where order.Company == "companies/1"
    orderby order.OrderedAt
    select order
    )
    .ToList();
````

Here RavenDB is instructed to use a 5 seconds time-out.

There are different strategies you could use here. To learn more about how to deal with stale indexes
I strongly recommend you to read the [official documentation](https://ravendb.net/docs/article-page/3.5/csharp/indexes/stale-indexes).

### Is staleness a real problem?
At first sight, the idea of a query that may not be fully up to date sounds scary. Right?
But in practice, this is how we almost always work in the real world.

Try this, call your bank and ask them how much money you have in your account (financial systems should be
consistent, right?). The answer you’ll get is going to be some variant of: “As of last business day, you had…”.

Much of management occurs with the previous day's data. Strategic decisions usually use even older data.
In practice, few queries need to reflect real-time data. So, why sacrifice computation power and
responsiveness generating data which nobody cares about?

## Timings

When testing your application, it's reasonable to check the timings. RavenDB can provide you
detailed timing information about the query parsing, Lucene processing, loading documents and transforming results as
you need.

> By default, detailed timings in queries are turned off, this is due to small overhead that calculation of such timings produces.

````csharp
RavenQueryStatistics stats;

var query = session.Query<Order>()
    .Statistics(out stats)
    .Customize(q => q.ShowTimings());

var orders = (
    from order in query
    where order.Company == "companies/1"
    orderby order.OrderedAt
    select order
    )
    .ToList();

var detailedInfo = stats.TimingsInMilliseconds;
````

The `TimingsInMilliseconds` property contains a `Dictionary<string, double>` object where each
entry corresponds to a specific timing.

## Exercise: Exploring timings results

This exercise picks up right where the previous one, in the [previous lesson](../lesson6/README.md), left off.
You already have the DocumentStoreHolder and one transformer.

Check out the following piece of code:

````csharp
static void Main(string[] args)
{
    new Products_ProductAndSupplierName().Execute(DocumentStoreHolder.Store);

    Console.Title = "Timings demo";
    using (var session = DocumentStoreHolder.Store.OpenSession())
    {
        RavenQueryStatistics stats;

        var query = session.Query<Order>()
            .Statistics(out stats)
            .Customize(q => q.ShowTimings());

        var orders = (
            from order in query
            orderby order.OrderedAt
            select order
            )
            .ToList();

        var detailedInfo = stats.TimingsInMilliseconds;

        Console.WriteLine($"Orders count : {orders.Count}");
        Console.WriteLine($"Total results: {stats.TotalResults}");
        Console.WriteLine("");

        Console.WriteLine($"Time (ms)  \t Element");
        foreach (var entry in detailedInfo)
        {
            Console.WriteLine($"{entry.Value} \t\t {entry.Key} ");
        }
    }
}
````

What are we doing here? It is just a regular query where the timings are enabled.

This is the output on my machine.

````
Orders count : 128
Total results: 830

Time (ms)        Element
16               Lucene search
1                Loading documents
0                Transforming results
0                Query parsing
````

As you can see, in this query, the transforming and the query parsing are almost inexpressive.


## Great Job!

**Congratulations! Now you know how to use and analyze the performance of indexes and transformers.**
