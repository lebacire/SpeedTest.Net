using System;
using System.Globalization;
using System.Linq;
using Xunit;

namespace SpeedTest.Tests
{
    public class SpeedTestTests
    {
        private readonly ISpeedTestClient _speedTestClientClient;

        public SpeedTestTests()
        {
            _speedTestClientClient = new SpeedTestClient();
        }

        [Fact]
        public async void Should_return_settings_with_sorted_server_list_by_distance()
        {
            var settings = await _speedTestClientClient.GetSettingsAsync();

            for (var i = 1; i < settings.Servers.Count; i++)
            {
                Assert.True(settings.Servers[i - 1].Distance.CompareTo(settings.Servers[i].Distance) <= 0);
            }
        }
        
        [Fact]
        public async void Should_return_settings_with_filtered_server_list_by_ignored_ids()
        {
            var settings = await _speedTestClientClient.GetSettingsAsync();

            var ignoredIds = settings.ServerConfig.IgnoreIds.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

            var servers = settings.Servers.Where(s => ignoredIds.Contains(s.Id.ToString(CultureInfo.InvariantCulture)));
            Assert.Empty(servers);
        }

        [Fact]
        public async void Should_test_latency_to_server()
        {
            var settings = await _speedTestClientClient.GetSettingsAsync();
            var latency = await _speedTestClientClient.TestServerLatencyAsync(settings.Servers.First());

            Assert.True(latency > 0);
            Assert.True(latency < 1000 * 60 * 5);
        }

        [Fact]
        public async void Should_test_download_speed()
        {
            var settings = await _speedTestClientClient.GetSettingsAsync();
            var speed = await _speedTestClientClient.TestDownloadSpeedAsync(settings.Servers.First(), settings.Download.ThreadsPerUrl);

            Assert.True(speed > 0);
        }

        [Fact]
        public async void Should_test_upload_speed()
        {
            var settings = await _speedTestClientClient.GetSettingsAsync();
            var speed = await _speedTestClientClient.TestUploadSpeedAsync(settings.Servers.First(), settings.Upload.ThreadsPerUrl);

            Assert.True(speed > 0);
        }
    }
}