using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Confluence2AzureDevOps.Base.CustomExceptions;
using Newtonsoft.Json;

namespace Confluence2AzureDevOps.Base
{
         /// <summary>
         /// Http API client
         /// </summary>
         public abstract class ApiClientBase
         {
             private const string APPLICATION_REQUEST_TYPE_JSON = "application/json";
     
             private const string UNKNOWN_API_EXCEPTION = "API calling: Internal error";
     
             private const string ACCESS_DENIED = "Access denied";
             
             private const string MISSING_ACCESS_TOKEN = "Missing Authorization Header";
             
             private const string DESERIALIZE_EXCEPTION = "Deserialize exception, cannot interpretation api response";
             
             protected string UrlBase { get; }

             protected ApiClientBase(string urlBase)
             {
                 UrlBase = urlBase;
             }
            
             #region - Post -

             /// <summary>
             /// Execute HTTP POST call
             /// </summary>
             /// <param name="url">Api address</param>
             /// <param name="data">Data to send (its will be transformed to JSON)</param>
             /// <param name="httpClientConfig">Http client configuration</param>
             /// <param name="applicationType">Application Type</param>
             /// <typeparam name="T">Expected returned Type</typeparam>
             /// <returns>Return an instance of expected type <see cref="T"/> </returns>
             /// <exception cref="UnautorizeApiException">Security exception. Its raise when api return an HTTP CODE 203</exception>
             /// <exception cref="ApiInvalidInputDataException">When input data is wrong. Its raise when api return an HTTP CODE 400</exception>
             /// <exception cref="ApiException">Its raise when api return an HTTP CODE is not expected.</exception>
             /// <exception cref="CallApiException">Component error. Raise when call and api. .i. e.: network exception.</exception>
             protected async Task<T> ApiPost<T>(string url, object data, HttpClientConfig  httpClientConfig = null, string applicationType = "application/json")
             {
                 T result;
     
                 try
                 {
                     var dataAsJson = JsonConvert.SerializeObject(data);
     
                     using (HttpClient client = CreateHttpClient(httpClientConfig))
                     {
                         StringContent stringContent =
                             new StringContent(dataAsJson, Encoding.UTF8, applicationType);
     
                         HttpResponseMessage requestResult = await client.PostAsync(url, stringContent);
     
                         result = await ReadAsyncRemoteContent<T>(requestResult);
                     }
                 }
                 catch (UnautorizeApiException)
                 {
                     throw;
                 }
                 catch (ApiInvalidInputDataException)
                 {
                     throw;
                 }
                 catch (ApiException)
                 {
                     throw;
                 }
                 catch (Exception ex)
                 {
                     var callApiException = new CallApiException(UNKNOWN_API_EXCEPTION, ex);
                     throw callApiException;
                 }
     
                 return result;
             }
             
              /// <summary>
             /// Execute HTTP POST call
             /// </summary>
             /// <param name="url">Api address</param>
             /// <param name="fileContent">Bite array from Json</param>
              /// <param name="queryString">Data to send into URL</param>
             /// <param name="httpClientConfig">Http client configuration</param>
             /// <param name="applicationType">Application Type</param>
             /// <typeparam name="T">Expected returned Type</typeparam>
             /// <returns>Return an instance of expected type <see cref="T"/> </returns>
             /// <exception cref="UnautorizeApiException">Security exception. Its raise when api return an HTTP CODE 203</exception>
             /// <exception cref="ApiInvalidInputDataException">When input data is wrong. Its raise when api return an HTTP CODE 400</exception>
             /// <exception cref="ApiException">Its raise when api return an HTTP CODE is not expected.</exception>
             /// <exception cref="CallApiException">Component error. Raise when call and api. .i. e.: network exception.</exception>
             protected async Task<T> ApiPutFile<T>(
                  string url, 
                  byte[] fileContent, 
                  Dictionary<string, string> queryString = null,
                  HttpClientConfig  httpClientConfig = null, 
                  string applicationType = "application/octet-stream")
             {
                 T result;
     
                 try
                 {
                     url = CreateUrlWithParameters(url, queryString);
                     string base64String = Convert.ToBase64String(fileContent);
                     
                     using (HttpClient client = CreateHttpClient(httpClientConfig))
                     {
                         StringContent stringContent = new StringContent(base64String, Encoding.UTF8, applicationType);
     
                         HttpResponseMessage requestResult = await client.PutAsync(url, stringContent);
     
                         result = await ReadAsyncRemoteContent<T>(requestResult);
                     }
                 }
                 catch (UnautorizeApiException)
                 {
                     throw;
                 }
                 catch (ApiInvalidInputDataException)
                 {
                     throw;
                 }
                 catch (ApiException)
                 {
                     throw;
                 }
                 catch (Exception ex)
                 {
                     var callApiException = new CallApiException(UNKNOWN_API_EXCEPTION, ex);
                     throw callApiException;
                 }
     
                 return result;
             }

             #endregion
             
             #region - Put -
             
             /// <summary>
             /// Execute HTTP POST call
             /// </summary>
             /// <param name="url">Api address</param>
             /// <param name="data">Data to send (its will be transformed to JSON)</param>
             /// <param name="queryString">Data to send into URL</param>
             /// <param name="httpClientConfig">Http client configuration</param>
             /// <typeparam name="T">Expected returned Type</typeparam>
             /// <returns>Return instance of <see cref="T"/></returns>
             /// <exception cref="UnautorizeApiException">Security exception. Its raise when api return an HTTP CODE 203</exception>
             /// <exception cref="ApiInvalidInputDataException">When input data is wrong. Its raise when api return an HTTP CODE 400</exception>
             /// <exception cref="ApiException">Its raise when api return an HTTP CODE is not expected.</exception>
             /// <exception cref="CallApiException">Component error. Raise when call and api. .i. e.: network exception.</exception>
             protected async Task<T> ApiPut<T>(string url, object data, Dictionary<string, string> queryString = null, HttpClientConfig  httpClientConfig = null)
             {
                 T result;
     
                 try
                 {
                     url = CreateUrlWithParameters(url, queryString);
                     
                     var dataAsJson = JsonConvert.SerializeObject(data);
     
                     using (HttpClient client = CreateHttpClient(httpClientConfig))
                     {
                         StringContent stringContent =
                             new StringContent(dataAsJson, Encoding.UTF8, APPLICATION_REQUEST_TYPE_JSON);
     
                         HttpResponseMessage requestResult = await client.PutAsync(url, stringContent);
     
                         result = await ReadAsyncRemoteContent<T>(requestResult);
                     }
                 }
                 catch (UnautorizeApiException)
                 {
                     throw;
                 }
                 catch (ApiInvalidInputDataException)
                 {
                     throw;
                 }
                 catch (ApiException)
                 {
                     throw;
                 }
                 catch (Exception ex)
                 {
                     var callApiException = new CallApiException(UNKNOWN_API_EXCEPTION, ex);
                     throw callApiException;
                 }
     
                 return result;
             }
             #endregion
             
             #region - Get -
             
             /// <summary>
             /// Execute HTTP POST call
             /// </summary>
             /// <param name="url">Api address</param>
             /// <param name="queryString">Data to send (its will be transformed to JSON)</param>
             /// <param name="httpClientConfig">Http client configuration</param>
             /// <typeparam name="T">Expected returned Type</typeparam>
             /// <returns>Return instance of <see cref="T"/></returns>
             /// <exception cref="CallApiException"></exception>
             protected async Task<T> ApiGet<T>(string url, Dictionary<string, string> queryString = null, HttpClientConfig  httpClientConfig = null)
             {
                 T result;
                     
                 try
                 {
                     url = CreateUrlWithParameters(url, queryString);
                     
                     using (HttpClient client = CreateHttpClient(httpClientConfig))
                     {
                         HttpResponseMessage requestResult = await client.GetAsync(url);

                         result = await ReadAsyncRemoteContent<T>(requestResult);
                     }
                 }
                 catch (UnautorizeApiException)
                 {
                     throw;
                 }
                 catch (ApiInvalidInputDataException)
                 {
                     throw;
                 }
                 catch (ApiException)
                 {
                     throw;
                 }
                 catch (Exception ex)
                 {
                     var callApiException = new CallApiException(UNKNOWN_API_EXCEPTION, ex);
                     throw callApiException;
                 }
     
                 return result;
             }
             
             #endregion

             private string CreateUrlWithParameters(string url, Dictionary<string, string> querystring)
             {
                 var query = new StringBuilder();

                 var urlToReq = new Uri(url);

                 if (querystring != null)
                 {
                     if (string.IsNullOrEmpty(urlToReq.Query))
                     {
                         query.Append("?");
                     }

                     foreach (var item in querystring)
                     {
                         query.AppendFormat($"&{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}");
                     }
                 }

                 url = $"{urlToReq.AbsoluteUri}{query}";

                 return url;
             }

             private async Task<T> ReadAsyncRemoteContent<T>(HttpResponseMessage requestResult)
             {
                 T result;
                 
                 string responseContent = await requestResult.Content.ReadAsStringAsync();
     
                 if (requestResult.StatusCode == HttpStatusCode.OK || requestResult.StatusCode == HttpStatusCode.Created)
                 {
                     try
                     {
                         result = JsonConvert.DeserializeObject<T>(responseContent);
                     }
                     catch (Exception serException)
                     {
                         var callApiException = new CallApiException(DESERIALIZE_EXCEPTION, serException);
                         throw callApiException;
                     }
                 }
                 else
                 {
                     var exception = ExtractException(requestResult.StatusCode, responseContent);
                     throw exception;
                 }

                 return result;
             }
             
             private Exception ExtractException(HttpStatusCode statusCode, string responseContent)
             {
                 Exception exceptionResult = null;
                 
                 if (statusCode == HttpStatusCode.NonAuthoritativeInformation)
                 {
                     exceptionResult = new UnautorizeApiException(MISSING_ACCESS_TOKEN);
                 }
                 if (statusCode == HttpStatusCode.Unauthorized)
                 {
                     exceptionResult = new UnautorizeApiException(ACCESS_DENIED);
                 }
                 
                 if (statusCode == HttpStatusCode.BadRequest)
                 {
                     var operationResultDto = JsonConvert.DeserializeObject<ApiErrorResult>(responseContent);
                     
                     exceptionResult = new ApiInvalidInputDataException(operationResultDto.Message, operationResultDto);
                 }
                 
                 if (statusCode == HttpStatusCode.InternalServerError)
                 {
                     var operationResultDto = JsonConvert.DeserializeObject<ApiErrorResult>(responseContent);
                     
                     exceptionResult = new ApiInvalidInputDataException(operationResultDto.Message, operationResultDto);
                 }
                 
                 if (exceptionResult == null)
                 {
                     ApiErrorResult errorResult = null;
                     
                     try
                     {
                         errorResult = JsonConvert.DeserializeObject<ApiErrorResult>(responseContent);
                     }
                     catch
                     {
                         errorResult = null;
                     }

                     if (errorResult != null)
                     {
                         exceptionResult = new ApiException("Api call error", errorResult);
                     }
                     else
                     {
                         exceptionResult = new ApiException(UNKNOWN_API_EXCEPTION);    
                     }
                     
                     exceptionResult.Data.Add("Service-HttpStatusCode", statusCode.ToString());
                     exceptionResult.Data.Add("Service-ResponseContent", responseContent);
                 }
     
                 return exceptionResult;
             }
     
             private HttpClient CreateHttpClient(HttpClientConfig  clientConfig = null)
             {
                 var client = new HttpClient();
     
                 if (clientConfig?.SecondsTimeout != null)
                 {
                     client.Timeout = TimeSpan.FromSeconds(clientConfig.SecondsTimeout.Value);
                 }

                 if (clientConfig?.AuthorizationValue != null)
                 {
                     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(clientConfig.AuthorizationType, clientConfig.AuthorizationValue);
                     
                 }
                
                 return client;
             }
         }
}