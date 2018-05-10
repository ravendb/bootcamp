# Unit 2, Lesson 6 - Goodbye Transformers, Welcome Server-Side Projections

In the previous lesson, you learned how to create shorter results from the server
using the `LoadDocument` server-side projections.

In this lesson, I will show you how to get shape results using server-side projections.

## What Happened with Transformers?

If you know RavenDB 3.5, you are probably looking for how to implement Transformers. 
Bad news: They are gone! Good news: You will not miss them.

Transformers were removed and substituted by a server-side projection support. Methods 
like `TransformWith` are no longer available and simple `Select` should be used instead. 

## Easy Start with Server-Side Projections

Instead of pulling full documents in query results, you can just grab some pieces of data 
from documents. You can also transform the projected results. 

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

And here it is the RQL:

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

We just created a function that will run on the server-side. 

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

To learn more about this functionality, I recommend you to read to read our article that covers 
the projection functionality which can be found [here](https://ravendb.net/docs/article-page/4.0/csharp/indexes/querying/projections).

**Let's move onto [Lesson 7](../lesson7/README.md).**