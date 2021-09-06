using afx.EmployesTime.Functions;
using afx.TestingApp.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace afx.TestingApp.Tests
{
    public class ScheduledFunctionGetConsolidateTest
    {
        [Fact]
        public async void ConsolidateTimesAsync_Should_Return_200()
        {
            //Arrenge
            MockCloudTableConsolidate mockTime = new MockCloudTableConsolidate(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            var date = DateTime.Now.ToString();

            ////Act
            _ = ScheduleGetConsolidateByDate.Run(null, mockTime, date, logger);
            string message = logger.Logs[0];

            //Assert
            Assert.Contains("Get all consolidate", message);
        }
    }
}
