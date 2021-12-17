exports = async function(changeEvent) {
  const fullDocument = changeEvent.fullDocument;
  const coll = context.services.get("demos").db("widgets").collection("shelves");
  const events = context.services.get("demos").db("eventstore").collection("events");
  
  if (fullDocument.type == "Widget.New") {
    coll.updateOne(
      { _id: fullDocument.details.shelf },
      { $inc: { widgets: 1 }},
      { upsert: true }
    );
  } else if (fullDocument.type == "Widget.Move") {
    events.find({ type: {$in: ["Widget.New", "Widget.Move"]}, entityUuid: fullDocument.entityUuid, ts: {$lt: fullDocument.ts }}).sort({ts:-1}).limit(1).toArray()
     .then(prevEvents => {
        let fromShelf = prevEvents[0].details.shelf;
    
        coll.updateOne(
          { _id: fromShelf },
          { $inc: { widgets: -1 }}
        );
        coll.updateOne(
          { _id: fullDocument.details.shelf },
          { $inc: { widgets: 1 }},
          { upsert: true }
        );
     });

  } else if (fullDocument.type == "Widget.Remove") {
    events.find({ type: {$in: ["Widget.New", "Widget.Move"]}, entityUuid: fullDocument.entityUuid, ts: {$lt: fullDocument.ts }}).sort({ts:-1}).limit(1).toArray()
      .then(prevEvents => {
        let fromShelf = prevEvents[0].details.shelf;
    
        coll.updateOne(
          { _id: fromShelf },
          { $inc: { widgets: -1 }}
        );
      });
   
  }
};
