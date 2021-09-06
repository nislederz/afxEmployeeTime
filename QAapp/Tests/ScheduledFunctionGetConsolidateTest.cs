using afx.EmployesTime.Functions;
using afx.TestingApp.Helpers;
using Microsoft.AspNetCore.Http.Internal;
using System;
using Xunit;

namespace afx.TestingApp.Tests
{
    public class ScheduledFunctionGetConsolidateTest
    {
        [Fact]
        public async void GetAllConsolidateByDate_Should_Return_200()
        {
            //Arrenge
            string date = DateTime.Now.ToString();
            MockCloudTableConsolidate mockTime = new MockCloudTableConsolidate(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(date);
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

            //Act
            RegisterTimes.GetAllConsolidateByDate(request, mockTime, date, logger);
            string message = logger.Logs[0];

            //Assert
            Assert.Contains("Get all consolidate", message);
        }
    }
}
