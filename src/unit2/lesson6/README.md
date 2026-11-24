# Unit 2, Lesson 6 - Goodbye Transformers, Welcome Server-Side Projections

In the previous lesson, you learned how to use `LoadDocument` to bring data from related
documents into an index.

In this lesson, I will show you how to shape query results on the server using projections.

## What Happened with Transformers?

If you used RavenDB 3.5, you may be looking for Transformers. They were removed
in RavenDB 4.0 and replaced by much better server-side projections. Methods like `TransformWith`
are no longer available. Use a plain `Select` instead.

## Easy Start with Server-Side Projections

Instead of returning full documents, a query can return only the pieces of data
you need. You can also compute new values from them.

A projection is applied to the results after the query has filtered, sorted and paged
them. This means you can't filter or sort on a projected field. `where` and `order by`
must use fields of the document (or of the index).

Let me share a short example:

```csharp
// request Name, City and Country 
// for all entities from 'Companies' collection
var results = session
    .Query<Company>()
    .Select(x => new
    {
        Name = x.Name,
        City = x.Address.City,
        Country = x.Address.Country
    })
    .ToList();
```

The related RQL is pretty simple as well:

```sql
from Companies
select Name, Address.City as City, Address.Country as Country
```
Another example? Here we go:

```csharp
var results = (from e in session.Query<Employee>()
               select new
               {
                   FullName = e.FirstName + " " + e.LastName,
               }).ToList();
```

And here is the RQL:

```sql
from Employees as e
select {
    FullName : e.FirstName + " " + e.LastName
}
```

## Getting Some More Advanced Results Using Functions

Let's do something more complex.

```csharp
var results = (from e in session.Query<Employee>()
               let format = 
                   (Func<Employee, string>)(p => 
                        p.FirstName + " " + p.LastName)
               select new
               {
                   FullName = format(e)
               }).ToList();
```

We just created a function that will run on the server side.

Let's look at the RQL.

```sql
declare function output(e) {
	var format = function(p){ return p.FirstName + " " + p.LastName; };
	return { FullName : format(e) };
}
from Employees as e select output(e)
```

Yes, we can define functions when coding with RQL! These
functions are pure JavaScript.

## Great job! 

To learn more, read the [projections article](https://docs.ravendb.net/5.4/indexes/querying/projections).

**Let's move onto [Lesson 7](../lesson7/README.md).**