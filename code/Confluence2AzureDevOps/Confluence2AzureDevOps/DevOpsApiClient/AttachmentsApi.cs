using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluence2AzureDevOps.Base.CustomExceptions;
using Confluence2AzureDevOps.ObjectModel.WikiPages;

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

        /// <summary>
        /// Upload new file, if file exists it not will be overwrite.
        /// <see cref="https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/attachments/create?view=azure-devops-rest-5.1"/>
        /// </summary>
        /// <param name="fileName">Wiki attachment name.</param>
        /// <param name="fileContent">Stream to upload</param>
        /// <returns>Created attachment info <see cref="DtWikiAttachment"/></returns>
        /// <exception cref="UnautorizeApiException">Security exception. Its raise when api return an HTTP CODE 203</exception>
        /// <exception cref="ApiInvalidInputDataException">When input data is wrong. Its raise when api return an HTTP CODE 400</exception>
        /// <exception cref="ApiException">Its raise when api return an HTTP CODE is not expected.</exception>
        /// <exception cref="CallApiException">Component error. Raise when call and api. .i. e.: network exception.</exception>
        public async Task<DtWikiAttachment> UploadFile(string fileName, byte[] fileContent)
        {
            DtWikiAttachment req = null;
            
            var queryString = new Dictionary<string, string>();
            
            queryString.Add("name", fileName);

            try
            {
                req = await ApiPutFile<DtWikiAttachment>(
                    UrlBase,
                    fileContent,
                    queryString: queryString,
                    httpClientConfig: HttpClientConfig);
            }
            catch (ApiInvalidInputDataException invalidInput)
            {
                if (invalidInput.Message.Contains("already exists"))
                {
                    req = new DtWikiAttachment()
                    {
                        Name = fileName,
                        Path = $"/.attachments/{fileName}"
                    };
                }
                else
                {
                    throw;
                }
            }

            return req;
        }
    }
}