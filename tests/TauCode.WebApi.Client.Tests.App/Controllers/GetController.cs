﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using TauCode.WebApi.Client.Tests.App.Dto;

namespace TauCode.WebApi.Client.Tests.App.Controllers
{
    [ApiController]
    public class GetController : ControllerBase
    {
        [HttpGet]
        [Route("get-from-route/{name}/{salary}/{bornAt}")]
        public IActionResult GetFromRoute(
            [FromRoute]string name,
            [FromRoute]decimal salary,
            [FromRoute]DateTime bornAt)
        {
            var person = new PersonDto
            {
                Name = name,
                Salary = salary,
                BornAt = bornAt,
            };

            return this.Ok(person);
        }

        [HttpGet]
        [Route("get-returns-notfound")]
        public IActionResult GetReturnsNotFound()
        {
            return this.NotFound(new
            {
                firstProp = "first-prop",
                secondProp = "second-prop"
            });
        }

        [HttpGet]
        [Route("get-returns-desired-generic-statuscode")]
        public IActionResult GetReturnsDesiredGenericStatusCode(
            [FromQuery] HttpStatusCode desiredStatusCode,
            [FromQuery] string desiredContent)
        {
            return new ContentResult
            {
                StatusCode = (int)desiredStatusCode,
                Content = desiredContent,
            };
        }

        [HttpGet]
        [Route("get-returns-notfound-error")]
        public IActionResult GetReturnsNotFoundError([FromQuery]string desiredCode, [FromQuery]string desiredMessage)
        {
            this.Response.Headers.Add(DtoHelper.PayloadTypeHeaderName, DtoHelper.ErrorPayloadType);

            return this.NotFound(new ErrorDto
            {
                Code = desiredCode,
                Message = desiredMessage,
            });
        }

        [HttpGet]
        [Route("get-returns-badrequest-error")]
        public IActionResult GetReturnsBadRequestError([FromQuery]string desiredCode, [FromQuery]string desiredMessage)
        {
            this.Response.Headers.Add(DtoHelper.PayloadTypeHeaderName, DtoHelper.ErrorPayloadType);

            return this.BadRequest(new ErrorDto
            {
                Code = desiredCode,
                Message = desiredMessage,
            });
        }
    }
}