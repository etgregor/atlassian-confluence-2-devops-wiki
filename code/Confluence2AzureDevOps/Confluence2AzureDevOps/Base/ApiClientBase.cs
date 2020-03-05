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
         /// Componente base para implementar un cliente de acceso a la API de BeerML.
         /// </summary>
         public abstract class ApiClientBase
         {
             protected string UrlBase { get; }

             protected ApiClientBase(string urlBase)
             {
                 UrlBase = urlBase;
             }
                 
             private const string ID_HEADER_TOKEN = "Authorization";
             
             private const string APPLICATION_REQUEST_TYPE_JSON = "application/json";
     
             private const string UNKNOW_API_EXCEPTION = "API calling: Internal error";
     
             protected const string ACCESS_DENIED = "Access denied";
             
             protected const string MISSING_ACCES_TOKEN = "Missing Authorization Header";
             
             private const string DESERIALIZ_EXCEPTION = "Deserialize exception, cannot interpretation api response";
     
             #region - Post -
     
             /// <summary>
             /// Hace una llamada Post a la API
             /// </summary>
             /// <param name="url">Url a donde se hace manda el post</param>
             /// <param name="data">Datos que se envían en la llamada</param>
             /// <param name="clientConfig"></param>
             /// <typeparam name="T">Tipo de objeto esperado</typeparam>
             /// <returns>Regresa instancas del tipo de objeto esperado</returns>
             /// <exception cref="UnautorizeApiException">Cuando el token es inválido</exception>
             /// <exception cref="ApiException">Cuando ocurren un error al mandar a llamar a la API</exception>
             /// <exception cref="CallApiException"></exception>
             protected async Task<T> ApiPost<T>(string url, object data, HttpClientConfig  clientConfig = null)
             {
                 T result;
     
                 try
                 {
                     var dataAsJson = JsonConvert.SerializeObject(data);
     
                     using (HttpClient client = CreateHttpClient(clientConfig))
                     {
                         StringContent stringContent =
                             new StringContent(dataAsJson, Encoding.UTF8, APPLICATION_REQUEST_TYPE_JSON);
     
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
                     var callApiException = new CallApiException(UNKNOW_API_EXCEPTION, ex);
                     throw callApiException;
                 }
     
                 return result;
             }
             
             /// <summary>
             /// Sube un archivo a un servicio rest
             /// </summary>
             /// <param name="url">Url a donde se hará el post</param>
             /// <param name="fieldNameOfFileInForm">Nombre del parámetro que contiene el archivo</param>
             /// <param name="filename">Nombre del archivo</param>
             /// <param name="fileAsBites">Contenido del archivo en bites</param>
             /// <param name="formFiels">Campos adicionales del formulario</param>
             /// <param name="clientConfig">Rest client config</param>
             /// <typeparam name="T">Tipo de dato esperado</typeparam>
             /// <returns>instancia de <see cref="T"/></returns>
             /// <exception cref="UnautorizeApiException">Cuando el token es inválido</exception>
             /// <exception cref="ApiException">Cuando ocurren un error al mandar a llamar a la API</exception>
             /// <exception cref="CallApiException"></exception>
             protected async Task<T> PotFile<T>(string url, string fieldNameOfFileInForm, string filename, byte[] fileAsBites, Dictionary<string, string> formFiels = null, HttpClientConfig  clientConfig = null)
             {
                 T result;
     
                 try
                 {
                     using (HttpClient client = CreateHttpClient(clientConfig))
                     {
                         using (MultipartFormDataContent form = new MultipartFormDataContent())
                         { 
                             if (formFiels != null)
                             {
                                 foreach (var data in formFiels)
                                 {
                                     form.Add(new StringContent(data.Value), data.Key);
                                 }
                             }
     
                             form.Add(new ByteArrayContent(fileAsBites, 0, fileAsBites.Length), fieldNameOfFileInForm, filename);
                             HttpResponseMessage requestResult = await client.PostAsync(url, form);
     
                             string responseContent = await requestResult.Content.ReadAsStringAsync();
     
                             if (requestResult.StatusCode == HttpStatusCode.OK)
                             {
                                 result = JsonConvert.DeserializeObject<T>(responseContent);
                             }
                             else
                             {
                                 var exception = ExtractException(requestResult.StatusCode, responseContent);
                                 throw exception;
                             }
                         }
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
                     var callApiException = new CallApiException(UNKNOW_API_EXCEPTION, ex);
                     throw callApiException;
                 }
     
                 return result;
             }
             
             #endregion
             
             #region - Put -
             
             protected async Task<T> ApiPut<T>(string url, object data, Dictionary<string, string> queryString = null, HttpClientConfig  clientConfig = null)
             {
                 T result;
     
                 try
                 {
                     url = CreateUrlWithParameters(url, queryString);
                     
                     var dataAsJson = JsonConvert.SerializeObject(data);
     
                     using (HttpClient client = CreateHttpClient(clientConfig))
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
                     var callApiException = new CallApiException(UNKNOW_API_EXCEPTION, ex);
                     throw callApiException;
                 }
     
                 return result;
             }
             #endregion
             
             #region - Get -
             
             protected async Task<T> ApiGet<T>(string url, Dictionary<string, string> querystring = null, HttpClientConfig  clientConfig = null)
             {
                 T result;
                     
                 try
                 {
                     url = CreateUrlWithParameters(url, querystring);
                     
                     using (HttpClient client = CreateHttpClient(clientConfig))
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
                     var callApiException = new CallApiException(UNKNOW_API_EXCEPTION, ex);
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
                         var callApiException = new CallApiException(DESERIALIZ_EXCEPTION, serException);
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
                     exceptionResult = new UnautorizeApiException(MISSING_ACCES_TOKEN);
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
                 
                 if (exceptionResult == null)
                 {
                     exceptionResult = new ApiException(UNKNOW_API_EXCEPTION);
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