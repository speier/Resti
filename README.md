## Resti - Simple REST handler for quick prototyping

[Resti](https://github.com/speier/resti) is a simple REST HTTP handler for quick prototyping.

Usage example:

```
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

Requirements:
Newtonsoft Json.NET (4.0.7)
