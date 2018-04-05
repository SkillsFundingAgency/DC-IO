﻿using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.IO.Dictionary.Test
{
    public class UnitTestDictionary : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;

        public UnitTestDictionary(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact]
        public async Task TestSet()
        {
            const string Key = "1_2_3_Set";
            const string Data = "Test Data";
            var loggerMock = new Mock<ILogger>();

            var service = new DictionaryKeyValuePersistenceService(loggerMock.Object);
            await service.SaveAsync(Key, Data);

            service._dictionary.TryGetValue(Key, out string value).Should().BeTrue();
            value.Should().Be(Data);
        }

        [Fact]
        public async Task TestGet()
        {
            const string Key = "1_2_3_Get";
            const string Data = "Test Data";
            var loggerMock = new Mock<ILogger>();

            var service = new DictionaryKeyValuePersistenceService(loggerMock.Object);
            service._dictionary[Key] = Data;

            string ret = await service.GetAsync(Key);

            ret.Should().Be(Data);
        }

        [Fact]
        public async Task TestRemove()
        {
            const string Key = "1_2_3_Remove";
            const string Data = "Test Data";
            var loggerMock = new Mock<ILogger>();

            var service = new DictionaryKeyValuePersistenceService(loggerMock.Object);
            service._dictionary[Key] = Data;

            await service.RemoveAsync(Key);

            service._dictionary.ContainsKey(Key).Should().BeFalse();
        }
    }
}
