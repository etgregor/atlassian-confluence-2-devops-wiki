using Confluence2AzureDevOps.Base;

namespace Confluence2AzureDevOps.DevOpsApiClient
{
    /// <summary>
    /// Azure DevOps client API base <see cref="https://docs.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-5.1"/>
    /// </summary>
    public class DevOpsApiBase : ApiClientBase
    {
        protected HttpClientConfig HttpClientConfig { get; }

        /// <summary>
        /// Initialize client whit PAT ( Personal Access Token)
        /// </summary>
        /// <param name="organization">The name of the Azure DevOps organization.</param>
        /// <param name="project">Project ID or project name</param>
        /// <param name="wikiIdentifier">Wiki Id or name.</param>
        /// <param name="personalAccessToken">PAT (DevOps personal access token)</param>
        /// <param name="apiVersion">Version of the API to use. This should be set to '5.1' to use this version of the api.</param>
        protected DevOpsApiBase(string organization, string project, string wikiIdentifier, string personalAccessToken, string apiVersion)
            : base(
                $"https://dev.azure.com/{organization}/{project}/_apis/wiki/wikis/{wikiIdentifier}/pages?api-version={apiVersion}")
        {
            string patToken = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1")
                .GetBytes(personalAccessToken + ":" + personalAccessToken));

            HttpClientConfig = new HttpClientConfig {AuthorizationType = "Basic", AuthorizationValue = patToken};
        }
    }
}