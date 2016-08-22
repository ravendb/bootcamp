# Unit 2, Lesson 7 - Statistics and some words about stale indexes! 

At this point, you know a lot about storing, loading and querying documents using
RavenDB, and that is awesome. Right?

In this lesson you will learn how to get details of a just executed query.  

## Introducing `RavenQueryStatics`

Every time you perform a query using the client API, you can request
some additional info using the `Statistics` method.

````csharp
RavenQueryStatics stats;

var orders = (
    from order in session.Query<Order>().Statistics(out stats)
    where order.Company == "companies/1"
    orderby order.OrderedAt
    select order
    ).ToList();

Console.WriteLine($"Index used was: {stats.IndexName}");
````

The results are avaiable via a [`RavenQueryStatics` object](https://ravendb.net/docs/article-page/latest/all/glossary/raven-query-statistics). 
Among the details you can get from the query statistics, you have: 

* Whatever the index was stale or not.
* The duration of the query on the server side.
* The total number of results (regardless of paging).
* The name of the index that this query run against.
* The last document etag indexed by the index.
* The timestamp of the last document indexed by the index.

## Stale index?

This is a very important fact: **RavenDB supports fully ACID writes but BASE reads**. 

RavenDB performs indexing in a background thread, which is executed whenever the new data comes in 
or the existing data is updated. This allows the server to respond quickly, even when large 
amounts of data have been changed, however in that case you may query stale indexes and, because
of that, some query results may not be fully up to date. Usually, the time between a document being
updated and the relevant indexes being updated is measured in milliseconds.

### How to know if a index is stale?

RavenDB is honest and clear about stale indexes. It is really easy to know if a query used a 
stale index just checking the statistics.

````csharp
RavenQueryStatics stats;

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
RavenQueryStatics stats;

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

There is different strategies you could use here. To learn more about how to deal with stale index
I strongly recommend you to read the [official documentation](https://ravendb.net/docs/article-page/3.5/csharp/indexes/stale-indexes).

## Timings

When testing your application is reasonable to check the timings. RavenDB can provide you 
detailed timing information about the query parsing, lucene processing, loading documents and transforming results as 
you need.

> By default, detailed timings in queries are turned off, this is due to small overhead that calculation of such timings produces.

````csharp
RavenQueryStatics stats;

var query = session.Query<Order>()
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

The `TimingsInMilliseconds` property contains an `Dictionary<string, double>` object where each
entry corresponds to a specific timing.

## Great Job!

**Congratulations! Now you know how to use and analyze the performance of indexes and transformers.**




