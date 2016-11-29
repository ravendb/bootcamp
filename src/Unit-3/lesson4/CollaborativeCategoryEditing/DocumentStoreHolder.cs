using System;
using Raven.Client;
using Raven.Client.Document;

namespace CollaborativeCategoryEditing
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