# Migration tool from Atlassian To Microsoft Azure DevOps

- Migrate Wiki
    - From Html to Md files

## Pre Requirements
- Account at Microsoft Azure DevOps whit read/write wiki.
- You need to have your .zip file with Html files hum has exported from *_Atlassian JiraCloud_*

## For Development

### Tools
.Net core

### Dependencies
``` js
//nuget packages
newtonsoft
HtmlAgilityPack
reversemarkdown
```

### Test Patameters

| Key | Value |
| ------ | ----------- |
| organization| The name of the Azure DevOps organization. |
| project| Project ID or project name|
| wikiIdentifier| Wiki Id or name|
| personalAccesToken| Azure devops Personal acces token|

[Go to azure documentation](https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages/create%20or%20update?view=azure-devops-rest-5.1)

``` js
ss
```

