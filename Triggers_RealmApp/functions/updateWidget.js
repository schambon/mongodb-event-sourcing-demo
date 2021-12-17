exports = function(changeEvent) {
  
  console.log(EJSON.stringify(changeEvent));
  
  const details = changeEvent.fullDocument.details;
    
  const db = context.services.get("demos").db("widgets");
  const widgetColl = db.collection("widgets");
  
  if (changeEvent.fullDocument.type == "Widget.New") {
    widgetColl.insertOne(
      { _id: changeEvent.fullDocument.entityUuid,
        status: "INSTORE", 
        description: details.description,
        shelf: details.shelf,
        history: [ {ts: changeEvent.fullDocument.ts, event: "New", shelf: details.shelf } ]}
    );
  } else if (changeEvent.fullDocument.type == "Widget.Move") {
    widgetColl.updateOne(
      { _id: changeEvent.fullDocument.entityUuid},
      { $set: { "shelf": details.shelf},
        $push: { history: { ts: changeEvent.fullDocument.ts, event: "Move", shelf: details.shelf } }
      }
    );
  } else if (changeEvent.fullDocument.type == "Widget.Remove") {
    widgetColl.updateOne(
      { _id: changeEvent.fullDocument.entityUuid},
      { $set: {"status": "REMOVED", location: details.destination},
        $push: { history: { ts: changeEvent.fullDocument.ts, event: "Remove" }},
        $unset: {shelf: true}});
  }
  
};
