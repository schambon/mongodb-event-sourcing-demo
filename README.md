Event Sourcing with MongoDB Demo
================================

This simplistic demo shows how a basic Event sourcing with CQRS application could work in MongoDB.

The scenario implemented is that of a warehouse storing widgets on shelves. The following commands are supported:
* Add new widget
* Move widget to a different shelf
* Remove widget from the store

The following queries are supported:
* Get information about a widget (including ones no longer in the store)
* Get all widgets
* Get shelf utilization
* Get the total number of widgets on shelves

There are three components:

* EventApi: a .NET Core web service that exposes the API endpoints
* EventProcessor: a .NET Core worker service that monitors the event store to compute the query models
* Triggers_RealmApp: the same, but using MongoDB Atlas triggers rather than a hosted .NET Core application

Use either EventProcessor or Triggers_RealmApp, not both, otherwise you'll have strange issues.

Schematically, the Command endpoints simply push events to an event store collection, and the processors (either C# or Realm triggers) consumes the events to save them as view models (widgets / shelves).

Configure
---------

The .NET applications are configured using .NET usual practices - appsettings.json is where you want to configure connection strings, etc.
The Realm triggers are installed using [Realm CLI](https://docs.mongodb.com/realm/cli/). Simply create a new project with `realm-cli init` then upload the configuration with `realm-cli push`. Note you need to create a data source to an Atlas cluster with the name "demos".

