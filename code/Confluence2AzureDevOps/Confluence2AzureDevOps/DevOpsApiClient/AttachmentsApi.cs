using System.Collections.Generic;
using System.Threading.Tasks;
using Confluence2AzureDevOps.Entities.WikiPages;

namespace Confluence2AzureDevOps.DevOpsApiClient
{
    /// <summary>
    /// Creates an attachment in the wiki
    /// <see cref="https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/attachments/create?view=azure-devops-rest-5.1"/>
    /// </summary>
    public class AttachmentsApi: DevOpsApiBase
    {
        /// <summary>
        /// Initialize client whit PAT ( Personal Access Token)
        /// </summary>
        /// <param name="organization">The name of the Azure DevOps organization.</param>
        /// <param name="project">Project ID or project name</param>
        /// <param name="wikiIdentifier">Wiki Id or name.</param>
        /// <param name="personalAccessToken">PAT (DevOps personal access token)</param>
        /// <param name="apiVersion">Version of the API to use. This should be set to '5.1' to use this version of the api.</param>
        public AttachmentsApi(string organization, string project, string wikiIdentifier, string personalAccessToken, string apiVersion = "5.1") 
            : base(organization, project, wikiIdentifier, personalAccessToken,  "attachments" ,apiVersion)
        {
        }

        public async Task<DtWikiAttachment> UploadFile(string fileName, byte[] fileContent)
        {
            var queryString = new Dictionary<string, string>();
            
            queryString.Add("name", fileName);

            DtWikiAttachment req = await ApiPutFile<DtWikiAttachment>(
                UrlBase,
                fileContent,
                queryString: queryString,
                httpClientConfig: HttpClientConfig);

            return req;
        }
    }
}