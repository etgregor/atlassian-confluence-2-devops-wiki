# Migration tool from Atlassian To Microsoft Azure DevOps

- Wiki migration
    - Migrate wiki from [Confluence Cloud HTML format](https://confluence.atlassian.com/confcloud/export-content-to-word-pdf-html-and-xml-724764824.html) to [Microsoft Azure DevOps Wiki](https://azure.microsoft.com/en-us/services/devops/wiki/)

## Pre Requirements
- Microsoft Azure DevOps account with read/write wiki permission.
- Folder that you got from [Confluence Cloud HTML format](https://confluence.atlassian.com/confcloud/export-content-to-word-pdf-html-and-xml-724764824.html).

## For Development

### Tech
- .Net Core

#### Nuget packages dependences
``` 
newtonsoft
HtmlAgilityPack
reversemarkdown
```

### Test Patameters

| Key | Value |
| ------ | ----------- |
| organization|[The name of the Azure DevOps Organization](https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages/create%20or%20update?view=azure-devops-rest-5.1). |
| project| [Project ID or project name](https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages/create%20or%20update?view=azure-devops-rest-5.1)|
| wikiIdentifier|[Wiki Id or name](https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages/create%20or%20update?view=azure-devops-rest-5.1)|
| personalAccesToken| [Azure devops Personal Acces Token](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page) with read/write wiki permission |


``` js
ss
```

