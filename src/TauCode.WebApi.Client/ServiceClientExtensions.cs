﻿using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TauCode.WebApi.Client.Exceptions;

namespace TauCode.WebApi.Client
{
    // TODO all file. get rid of todo-s and notimpl.
    public static class ServiceClientExtensions
    {
        #region Private

        private static async Task<Exception> GetFailureException(HttpResponseMessage failMessage)
        {
            Exception ex = null;

            try
            {
                var content = await failMessage.Content.ReadAsStringAsync();
                var payloadType = failMessage.Headers.TryGetSingleHeader(DtoHelper.PayloadTypeHeaderName);

                if (payloadType == DtoHelper.ErrorPayloadType)
                {
                    var error = JsonConvert.DeserializeObject<ErrorDto>(content);

                    switch (failMessage.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            ex = new NotFoundErrorServiceClientException(error.Code, error.Message);
                            break;

                        default:
                            ex = new ErrorServiceClientException(failMessage.StatusCode, error.Code, error.Message);
                            break;
                    }
                }
                else if (payloadType == DtoHelper.ValidationErrorPayloadType)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    // Generic error.
                    ex = new HttpServiceClientException(failMessage.StatusCode, content);
                }

                //switch (failMessage.StatusCode)
                //{
                //    case HttpStatusCode.NotFound:
                //        if (payloadType == DtoHelper.ErrorPayloadType)
                //        {
                //            throw new NotImplementedException();

                //            //var error = JsonConvert.DeserializeObject<ErrorDto>(content);
                //            //ex = new NotFoundServiceClientException(error.Code, error.Message);
                //            break;
                //        }
                //        else
                //        {
                //            // Generic NotFound 404.
                //            ex = new HttpServiceClientException(HttpStatusCode.NotFound, content);
                //            break;
                //        }

                //    case HttpStatusCode.BadRequest:
                //        if (payloadType == DtoHelper.ValidationErrorPayloadType)
                //        {
                //            throw new NotImplementedException();

                //            //var validationError = JsonConvert.DeserializeObject<ValidationErrorDto>(content);
                //            //ex = new ValidationServiceClientException(validationError);
                //            //break;
                //        }
                //        else if (payloadType == DtoHelper.ErrorPayloadType)
                //        {
                //            throw new NotImplementedException();
                //        }
                //        else
                //        {
                //            throw new NotImplementedException();
                //        }


                //    // todo ALL
                //    //if (SubReasonIs(failMessage, DtoHelper.ValidationErrorSubReason))
                //    //{
                //    //    // validation error?
                //    //    var validationError = JsonConvert.DeserializeObject<ValidationErrorResponseDto>(content);
                //    //    ex = new ServiceRequestValidationException(validationError);
                //    //}
                //    //else
                //    //{
                //    //    ex = new ServiceBadRequestException("BadRequest", content);
                //    //}

                //    //break;

                //    case HttpStatusCode.Forbidden:
                //        throw new NotImplementedException();
                //    //ex = TryDeserializeAsServiceException<ServiceResourceForbiddenException>(
                //    //    failMessage,
                //    //    DtoHelper.ForbiddenErrorSubReason,
                //    //    content);

                //    //break;

                //    case HttpStatusCode.Conflict:
                //        if (payloadType == DtoHelper.ErrorPayloadType)
                //        {
                //            throw new NotImplementedException();
                //            //var error = JsonConvert.DeserializeObject<ErrorDto>(content);
                //            //ex = new ConflictServiceClientException(error.Code, error.Message);
                //            //break;
                //        }
                //        else
                //        {
                //            throw new NotImplementedException();
                //        }

                //        //ex = TryDeserializeAsServiceException<ServiceBusinessLogicException>(
                //        //    failMessage,
                //        //    DtoHelper.BusinessLogicErrorSubReason,
                //        //    content);

                //        //break;
                //}

                if (ex == null)
                {
                    throw new NotImplementedException();
                    //ex = new ServiceClientException($"{failMessage.StatusCode}", content);
                }

                return ex;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        private static string TryGetSingleHeader(this HttpHeaders headers, string headerName)
        {
            var gotHeader = headers.TryGetValues(headerName, out var values);
            if (!gotHeader)
            {
                return null;
            }

            var valueArray = values as string[] ?? values.ToArray();
            return valueArray.Length == 1 ? valueArray[0] : null;
        }

        #endregion

        public static async Task<TResult> GetAsync<TResult>(
            this IServiceClient serviceClient,
            string routeTemplate,
            object segments = null,
            object queryParams = null)
        {
            var response = await serviceClient.SendAsync(HttpMethod.Get, routeTemplate, segments, queryParams);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                try
                {
                    var content = JsonConvert.DeserializeObject<TResult>(json);
                    return content;
                }
                catch (Exception ex)
                {
                    throw new NotImplementedException();
                    //var clientEx = new ServiceClientException("DeserializationError", ex.Message); // todo: right?
                    //return await Task.FromException<TResult>(clientEx);
                }
            }
            else
            {
                var ex = await GetFailureException(response);
                return await Task.FromException<TResult>(ex);
            }
        }

        public static async Task PostAsync(
            this IServiceClient serviceClient,
            string routeTemplate,
            object segments = null,
            object queryParams = null,
            object body = null)
        {
            var response = await serviceClient.SendAsync(HttpMethod.Post, routeTemplate, segments, queryParams, body);

            if (!response.IsSuccessStatusCode)
            {
                var ex = await GetFailureException(response);
                await Task.FromException(ex);
            }
        }

        public static async Task PutAsync(
            this IServiceClient serviceClient,
            string routeTemplate,
            object segments = null,
            object queryParams = null,
            object body = null)
        {
            var response = await serviceClient.SendAsync(HttpMethod.Put, routeTemplate, segments, queryParams, body);

            if (!response.IsSuccessStatusCode)
            {
                var ex = await GetFailureException(response);
                await Task.FromException(ex);
            }
        }

        public static async Task DeleteAsync(
            this IServiceClient serviceClient,
            string routeTemplate,
            object segments = null,
            object queryParams = null)
        {
            var response = await serviceClient.SendAsync(HttpMethod.Delete, routeTemplate, segments, queryParams);

            if (!response.IsSuccessStatusCode)
            {
                var ex = await GetFailureException(response);
                await Task.FromException(ex);
            }
        }

        public static async Task<CreateResultDto> CreateAsync(
            this IServiceClient serviceClient,
            string routeTemplate,
            object segments = null,
            object queryParams = null,
            object body = null)
        {
            var response = await serviceClient.SendAsync(HttpMethod.Post, routeTemplate, segments, queryParams, body);

            if (!response.IsSuccessStatusCode)
            {
                var ex = await GetFailureException(response);
                return await Task.FromException<CreateResultDto>(ex);
            }

            var payloadType = response.Headers.TryGetSingleHeader(DtoHelper.PayloadTypeHeaderName);
            if (payloadType == DtoHelper.CreateResultPayloadType)
            {
                var json = await response.Content.ReadAsStringAsync();

                try
                {
                    var content = JsonConvert.DeserializeObject<CreateResultDto>(json);
                    return content;
                }
                catch (Exception ex)
                {
                    throw new NotImplementedException();
                    //var clientEx = new ServiceClientException("DeserializationError", ex.Message);
                    //return await Task.FromException<CreateResultDto>(clientEx);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            //string instanceId;
            //string route;

            //try
            //{
            //    instanceId = response.Headers.TryGetSingleHeader(DtoHelper.InstanceIdHeaderName);
            //    route = response.Headers.TryGetSingleHeader(DtoHelper.RouteHeaderName);
            //}
            //catch (Exception ex)
            //{
            //    return await Task.FromException<UpdateResult<T>>(ex);
            //}

            //T content;

            //var json = await response.Content.ReadAsStringAsync();

            //try
            //{
            //    content = JsonConvert.DeserializeObject<T>(json);
            //}
            //catch (Exception ex)
            //{
            //    var clientEx = new ServiceClientException("DeserializationError", ex.Message);
            //    return await Task.FromException<UpdateResult<T>>(clientEx);
            //}

            //var updateResult = new UpdateResult<T>
            //{
            //    StatusCode = response.StatusCode,
            //    InstanceId = instanceId,
            //    Route = route,
            //    Content = content,
            //};

            //return updateResult;
        }

        public static async Task<UpdateResultDto> UpdateAsync(
            this IServiceClient serviceClient,
            string routeTemplate,
            object segments = null,
            object queryParams = null,
            object body = null)
        {
            var response = await serviceClient.SendAsync(HttpMethod.Put, routeTemplate, segments, queryParams, body);

            if (!response.IsSuccessStatusCode)
            {
                var ex = await GetFailureException(response);
                return await Task.FromException<UpdateResultDto>(ex);
            }

            var payloadType = response.Headers.TryGetSingleHeader(DtoHelper.PayloadTypeHeaderName);
            if (payloadType == DtoHelper.UpdateResultPayloadType)
            {
                var json = await response.Content.ReadAsStringAsync();

                try
                {
                    var content = JsonConvert.DeserializeObject<UpdateResultDto>(json);
                    return content;
                }
                catch (Exception ex)
                {
                    throw new NotImplementedException();
                    //var clientEx = new ServiceClientException("DeserializationError", ex.Message);
                    //return await Task.FromException<UpdateResultDto>(clientEx);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            //string instanceId;
            //string route;

            //try
            //{
            //    instanceId = response.Headers.TryGetSingleHeader(DtoHelper.InstanceIdHeaderName);
            //    route = response.Headers.TryGetSingleHeader(DtoHelper.RouteHeaderName);
            //}
            //catch (Exception ex)
            //{
            //    return await Task.FromException<UpdateResult<T>>(ex);
            //}

            //T content;

            //var json = await response.Content.ReadAsStringAsync();

            //try
            //{
            //    content = JsonConvert.DeserializeObject<T>(json);
            //}
            //catch (Exception ex)
            //{
            //    var clientEx = new ServiceClientException("DeserializationError", ex.Message);
            //    return await Task.FromException<UpdateResult<T>>(clientEx);
            //}

            //var updateResult = new UpdateResult<T>
            //{
            //    StatusCode = response.StatusCode,
            //    InstanceId = instanceId,
            //    Route = route,
            //    Content = content,
            //};

            //return updateResult;
        }

        public static async Task<CreateResultDto<T>> CreateWithResultAsync<T>(
            this IServiceClient serviceClient,
            string routeTemplate,
            object segments = null,
            object queryParams = null,
            object body = null)
        {
            var response = await serviceClient.SendAsync(HttpMethod.Post, routeTemplate, segments, queryParams, body);

            if (!response.IsSuccessStatusCode)
            {
                var ex = await GetFailureException(response);
                return await Task.FromException<CreateResultDto<T>>(ex);
            }

            var payloadType = response.Headers.TryGetSingleHeader(DtoHelper.PayloadTypeHeaderName);
            if (payloadType == DtoHelper.CreateResultPayloadType)
            {
                var json = await response.Content.ReadAsStringAsync();

                try
                {
                    var content = JsonConvert.DeserializeObject<CreateResultDto<T>>(json);
                    return content;
                }
                catch (Exception ex)
                {
                    throw new NotImplementedException();
                    //var clientEx = new ServiceClientException("DeserializationError", ex.Message);
                    //return await Task.FromException<CreateResultDto<T>>(clientEx);
                }
            }
            else
            {
                throw new NotImplementedException();
            }


            //try
            //{
            //    instanceId = response.Headers.TryGetSingleHeader(DtoHelper.InstanceIdHeaderName);
            //    route = response.Headers.TryGetSingleHeader(DtoHelper.RouteHeaderName);
            //}
            //catch (Exception ex)
            //{
            //    return await Task.FromException<CreateResult<T>>(ex);
            //}

            //T content;

            //var json = await response.Content.ReadAsStringAsync();

            //try
            //{
            //    content = JsonConvert.DeserializeObject<T>(json);
            //}
            //catch (Exception ex)
            //{
            //    var clientEx = new ServiceClientException("DeserializationError", ex.Message);
            //    return await Task.FromException<CreateResult<T>>(clientEx);
            //}

            //var createResult = new CreateResult<T>
            //{
            //    StatusCode = response.StatusCode,
            //    InstanceId = instanceId,
            //    Route = route,
            //    Location = response.Headers.Location,
            //    Content = content,
            //};

            //return createResult;
        }

        public static async Task<UpdateResultDto<T>> UpdateWithResultAsync<T>(
            this IServiceClient serviceClient,
            string routeTemplate,
            object segments = null,
            object queryParams = null,
            object body = null)
        {
            var response = await serviceClient.SendAsync(HttpMethod.Put, routeTemplate, segments, queryParams, body);

            if (!response.IsSuccessStatusCode)
            {
                var ex = await GetFailureException(response);
                return await Task.FromException<UpdateResultDto<T>>(ex);
            }

            var payloadType = response.Headers.TryGetSingleHeader(DtoHelper.PayloadTypeHeaderName);
            if (payloadType == DtoHelper.UpdateResultPayloadType)
            {
                var json = await response.Content.ReadAsStringAsync();

                try
                {
                    var content = JsonConvert.DeserializeObject<UpdateResultDto<T>>(json);
                    return content;
                }
                catch (Exception ex)
                {
                    throw new NotImplementedException();
                    //var clientEx = new ServiceClientException("DeserializationError", ex.Message);
                    //return await Task.FromException<UpdateResultDto<T>>(clientEx);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            //string instanceId;
            //string route;

            //try
            //{
            //    instanceId = response.Headers.TryGetSingleHeader(DtoHelper.InstanceIdHeaderName);
            //    route = response.Headers.TryGetSingleHeader(DtoHelper.RouteHeaderName);
            //}
            //catch (Exception ex)
            //{
            //    return await Task.FromException<UpdateResult<T>>(ex);
            //}

            //T content;

            //var json = await response.Content.ReadAsStringAsync();

            //try
            //{
            //    content = JsonConvert.DeserializeObject<T>(json);
            //}
            //catch (Exception ex)
            //{
            //    var clientEx = new ServiceClientException("DeserializationError", ex.Message);
            //    return await Task.FromException<UpdateResult<T>>(clientEx);
            //}

            //var updateResult = new UpdateResult<T>
            //{
            //    StatusCode = response.StatusCode,
            //    InstanceId = instanceId,
            //    Route = route,
            //    Content = content,
            //};

            //return updateResult;

        }

        public static async Task<DeleteResultDto> DeleteWithResultAsync(
            this IServiceClient serviceClient,
            string routeTemplate,
            object segments = null,
            object queryParams = null)
        {
            var response = await serviceClient.SendAsync(HttpMethod.Delete, routeTemplate, segments, queryParams);

            if (!response.IsSuccessStatusCode)
            {
                var ex = await GetFailureException(response);
                return await Task.FromException<DeleteResultDto>(ex);
            }

            var payloadType = response.Headers.TryGetSingleHeader(DtoHelper.PayloadTypeHeaderName);
            if (payloadType == DtoHelper.DeleteResultPayloadType)
            {
                var json = await response.Content.ReadAsStringAsync();

                try
                {
                    var content = JsonConvert.DeserializeObject<DeleteResultDto>(json);
                    return content;
                }
                catch (Exception ex)
                {
                    throw new NotImplementedException();
                    //var clientEx = new ServiceClientException("DeserializationError", ex.Message);
                    //return await Task.FromException<DeleteResultDto>(clientEx);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
