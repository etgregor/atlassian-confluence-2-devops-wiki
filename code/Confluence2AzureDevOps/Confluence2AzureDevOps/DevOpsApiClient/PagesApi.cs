using System.Collections.Generic;
using System.Threading.Tasks;
using Confluence2AzureDevOps.Base;
using Confluence2AzureDevOps.Base.CustomExceptions;
using Confluence2AzureDevOps.Entities.WikiPages;

namespace Confluence2AzureDevOps.DevOpsApiClient
{
    public class PagesApi : ApiClientBase
    {
        private readonly HttpClientConfig _servConfig;

        public PagesApi(string organization, string project, string wikiIdentifier, string personalAccesToken)
            : base(
                $"https://dev.azure.com/{organization}/{project}/_apis/wiki/wikis/{wikiIdentifier}/pages?api-version=5.1")
        {
            string patToken = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(personalAccesToken + ":" + personalAccesToken));

            _servConfig = new HttpClientConfig {AuthorizationType = "Basic", AuthorizationValue = patToken};
        }

        /// <summary>
        /// Gets metadata or content of the wiki page for the provided path. Content negotiation is done based on the Accept header sent in the request.
        /// <see cref="https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages/get%20page?view=azure-devops-rest-5.1"/>
        /// </summary>
        /// <returns>Returns <see cref="DtWikiPage"/></returns>
        /// <exception cref="UnautorizeApiException">Security exception. Its raise when api return an HTTP CODE 203</exception>
        /// <exception cref="ApiInvalidInputDataException">When input data is wrong. Its raise when api return an HTTP CODE 400</exception>
        /// <exception cref="ApiException">Its raise when api return an HTTP CODE is not expected.</exception>
        /// <exception cref="CallApiException">Component error. Raise when call and api. .i. e.: network exception.</exception>
        public async Task<DtWikiPage> GetFullTree()
        {
            var queryString = new Dictionary<string, string>();
            queryString.Add("recursionLevel", "full");

            DtWikiPage page = await ApiGet<DtWikiPage>(
                UrlBase,
                querystring:
                queryString, clientConfig: _servConfig);

            return page;
        }
        
        /// <summary>
        /// Creates or edits a wiki page.
        /// <see cref="https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages/create%20or%20update?view=azure-devops-rest-5.1"/>
        /// </summary>
        /// <param name="pageInfo">Page creation info.</param>
        /// <returns>Return resent create <see cref="DtWikiPage"/></returns>
        /// <exception cref="UnautorizeApiException">Security exception. Its raise when api return an HTTP CODE 203</exception>
        /// <exception cref="ApiInvalidInputDataException">When input data is wrong. Its raise when api return an HTTP CODE 400</exception>
        /// <exception cref="ApiException">Its raise when api return an HTTP CODE is not expected.</exception>
        /// <exception cref="CallApiException">Component error. Raise when call and api. .i. e.: network exception.</exception>
        public async Task<DtWikiPage> CreateOrUpdatePage(PageWikiCreateOptions pageInfo)
        {
            var pageContent = new {content = pageInfo.Content};
            
            var queryString = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(pageInfo.Path))
            {
                queryString.Add("path", pageInfo.Path);
            }
            
            if (!string.IsNullOrEmpty(pageInfo.Comment))
            {
                queryString.Add("comment", pageInfo.Comment);
            }

            DtWikiPage page = await ApiPut<DtWikiPage>(
                UrlBase, 
                pageContent, 
                queryString: queryString,
                clientConfig: _servConfig);

            return page;
        }
    }
}