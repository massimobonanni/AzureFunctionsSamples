using AppConfigSyncFunction.Events;
using AppConfigSyncFunction.Interfaces;
using Azure.Storage.Queues.Models;
using Castle.Core.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using IMSLogger = Microsoft.Extensions.Logging.ILogger;

namespace AppConfigSyncFunction.Tests
{
    public class AppConfigSyncTest
    {
        #region [ ctor ]
        [Fact]
        public void ctor_AllArgumentsNotNull_CreateInstance()
        {
            var appConfigSyncServiceMock = new Mock<IAppConfigurationSyncService>();
            var queueServiceMock = new Mock<IEventsService>();
            var configurationMock = new Mock<IConfiguration>();

            var target = new AppConfigSync(appConfigSyncServiceMock.Object,
                queueServiceMock.Object,
                configurationMock.Object);

            Assert.NotNull(target);
        }

        [Fact]
        public void ctor_AppConfigSyncServiceNull_ThrowException()
        {
            IAppConfigurationSyncService appConfigSyncService = null;
            var queueServiceMock = new Mock<IEventsService>();
            var configurationMock = new Mock<IConfiguration>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                var target = new AppConfigSync(appConfigSyncService,
                    queueServiceMock.Object,
                    configurationMock.Object);
            });
        }

        [Fact]
        public void ctor_QueueServiceNull_ThrowException()
        {
            var appConfigSyncServiceMock = new Mock<IAppConfigurationSyncService>();
            IEventsService queueService = null;
            var configurationMock = new Mock<IConfiguration>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                var target = new AppConfigSync(appConfigSyncServiceMock.Object,
                    queueService,
                    configurationMock.Object);
            });
        }

        [Fact]
        public void ctor_ConfigurationNull_ThrowException()
        {
            var appConfigSyncServiceMock = new Mock<IAppConfigurationSyncService>();
            var queueServiceMock = new Mock<IEventsService>();
            IConfiguration configuration = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                var target = new AppConfigSync(appConfigSyncServiceMock.Object,
                    queueServiceMock.Object,
                    configuration);
            });
        }
        #endregion [ ctor ]

        #region [ AppConfigSyncFunction ]
        [Fact]
        public async Task AppConfigSyncFunction_NoMessagesInQueue_NoCallsBetweenAppConfigurations()
        {
            var appConfigSyncServiceMock = new Mock<IAppConfigurationSyncService>();
            var queueServiceMock = new Mock<IEventsService>();
            var configurationMock = new Mock<IConfiguration>();
            var loggerMock = new Mock<IMSLogger>();

            var timeInfo = TestUtility.GenerateTimerInfo();

            queueServiceMock
                .Setup(e => e.ReceiveEventsAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Event>())
                .Verifiable();

            appConfigSyncServiceMock
                .Setup(e => e.Connect(null, null))
                .Verifiable();

            var target = new AppConfigSync(appConfigSyncServiceMock.Object,
                queueServiceMock.Object,
                configurationMock.Object);

            await target.AppConfigSyncFunction(timeInfo, loggerMock.Object);

            queueServiceMock
                .Verify(e => e.ReceiveEventsAsync(It.IsAny<int>()), Times.Once);

            appConfigSyncServiceMock
                .Verify(e => e.Connect(null, null), Times.Never());

        }

        [Fact]
        public async Task AppConfigSyncFunction_OneMessagesInQueue_UpdateMessage_UpdateFromPrimaryToSecondary()
        {
            var appConfigSyncServiceMock = new Mock<IAppConfigurationSyncService>();
            var queueServiceMock = new Mock<IEventsService>();
            var configurationMock = new Mock<IConfiguration>();
            var loggerMock = new Mock<IMSLogger>();

            var timeInfo = TestUtility.GenerateTimerInfo();

            var modifiedEvent = TestUtility.CreateKeyValueModifiedEvent();

            queueServiceMock
                .SetupSequence(e => e.ReceiveEventsAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Event>() { modifiedEvent })
                .ReturnsAsync(new List<Event>());

            queueServiceMock
                .Setup(e => e.DeleteEventAsync(modifiedEvent))
                .Verifiable();

            appConfigSyncServiceMock
                .Setup(e => e.Connect(null, null))
                .Verifiable();

            appConfigSyncServiceMock
                .Setup(e => e.UpsertToSecondary(modifiedEvent, default))
                .ReturnsAsync(true)
                .Verifiable();

            var target = new AppConfigSync(appConfigSyncServiceMock.Object,
                queueServiceMock.Object,
                configurationMock.Object);

            await target.AppConfigSyncFunction(timeInfo, loggerMock.Object);

            queueServiceMock
                .Verify(e => e.ReceiveEventsAsync(It.IsAny<int>()), Times.Exactly(2));

            queueServiceMock
                .Verify(e => e.DeleteEventAsync(modifiedEvent), Times.Once);

            appConfigSyncServiceMock
                .Verify(e => e.Connect(null, null), Times.Once);

            appConfigSyncServiceMock
               .Verify(e => e.UpsertToSecondary(modifiedEvent, default), Times.Once);
        }
        #endregion [ AppConfigSyncFunction ]
    }
}
