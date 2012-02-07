## Resti - Simple REST handler for quick prototyping

[Resti](https://github.com/speier/Resti) is a simple REST HTTP handler for quick prototyping.

Usage example:

```c#
var api = new RestHandler();
 
api.Get["/api/reports/{guid}"] = x =>
{
  return ReportsRepository.GetReportsByGuid(guid);
}

api.Get["/api/documents/{id}"] = x =>
{
  return DocumentsRepository.GetDocumentById(id);
}
```

```javascript
$.get('api/documents/1', function (report) {
 // report..
});

Requirements:
Newtonsoft Json.NET (4.0.7)
