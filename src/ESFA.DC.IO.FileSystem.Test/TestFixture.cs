using System;
using System.IO;
using ESFA.DC.IO.FileSystem.Config.Interfaces;
using Moq;

namespace ESFA.DC.IO.FileSystem.Test
{
    public sealed class TestFixture : IDisposable
    {
        public IFileSystemKeyValuePersistenceServiceConfig Config { get; }

        public TestFixture()
        {
            var mock = new Mock<IFileSystemKeyValuePersistenceServiceConfig>();
            mock.SetupGet(x => x.Directory).Returns("Storage");
            Directory.CreateDirectory("Storage");
            Config = mock.Object;
        }

        public void Dispose()
        {
            Directory.Delete("Storage", true);
        }
    }
}
