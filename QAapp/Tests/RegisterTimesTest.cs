using afx.Common.Models;
using afx.EmployesTime.Functions;
using afx.TestingApp.Helpers;
using afxTimes.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace afx.TestingApp.Tests
{
    public class RegisterTimesTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateEmployeeTime_Should_Return_200()
        {
            //Arrenge
            MockCloudTableTime mockTime = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            EmployeeTime timeRequest = TestFactory.GetTimeRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(timeRequest);

            //Act
            IActionResult response = await RegisterTimes.CreateEmployeeTime(request, mockTime, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UpdateEmployeeTime_Should_Return_200()
        {
            //Arrenge
            MockCloudTableTime mockTodo = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            EmployeeTime timeRequest = TestFactory.GetTimeRequest();
            Guid timeId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(timeId, timeRequest);

            //Act
            IActionResult response = await RegisterTimes.UpdateEmployeeTime(request, mockTodo, timeId.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void DeleteEmployeeTime_Should_Return_200()
        {
            //Arrenge
            MockCloudTableTime mockTodo = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            EmployeeTime timeRequest = TestFactory.GetTimeRequest();
            Guid timeId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(timeId, timeRequest);
            EmployeesTimeEntity timeRequestEntity = TestFactory.GetTimeEntity();

            //Act
            IActionResult response = await RegisterTimes.DeleteEmployeeTime(request, timeRequestEntity, mockTodo, timeId.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void GetEmployeeTimeById_Should_Return_200()
        {
            //Arrenge
            EmployeeTime timeRequest = TestFactory.GetTimeRequest();
            Guid timeId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(timeId, timeRequest);
            EmployeesTimeEntity timeRequestEntity = TestFactory.GetTimeEntity();

            //Act
            IActionResult response =  RegisterTimes.GetEmployeeTimeById(request, timeRequestEntity, timeId.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void GetAllEmployeeTime_Should_Return_200()
        {
            //Arrenge
            MockCloudTableTime mockTime = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            DefaultHttpRequest request = TestFactory.CreateHttpRequest();

            //Act
            IActionResult response = await RegisterTimes.GetAllEmployeeTime(request, mockTime, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

    }
}
