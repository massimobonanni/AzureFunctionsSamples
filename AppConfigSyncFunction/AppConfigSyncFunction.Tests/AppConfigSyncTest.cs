using AppConfigSyncFunction.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using Xunit;

namespace AppConfigSyncFunction.Tests
{
    public class AppConfigSyncTest
    {
        #region [ ctor ]
        [Fact]
        public void ctor_AllArgumentsNotNull_CreateInstance()
        {
            var appConfigSyncServiceMock = new Mock<IAppConfigurationSyncService>();
            var queueServiceMock = new Mock<IQueueService>();
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
            var queueServiceMock = new Mock<IQueueService>();
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
            IQueueService queueService = null;
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
            var queueServiceMock = new Mock<IQueueService>();
            IConfiguration configuration = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                var target = new AppConfigSync(appConfigSyncServiceMock.Object,
                    queueServiceMock.Object,
                    configuration);
            });
        }
        #endregion [ ctor ]

    }
}
