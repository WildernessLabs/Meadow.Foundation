using Meadow.Foundation.Serialization;
using System;
using Xunit;

namespace Unit.Tests;

public class NullablePropertyTests
{
    [Fact]
    public void DeserializeNullablePropertyTest()
    {
        var message = new TestResultMessage
        {
            TestID = Guid.Parse("2e7df339-15fd-466d-8231-a71eae47df85"),
            CompletedTimestamp = DateTime.UtcNow,
            MeadowOSVersion = "0.1",
            StartedTimestamp = DateTime.UtcNow,
            TargetPlatform = "ProjectLab",
            TestName = "UnitTest",
            TestRunBy = "Test Runner",
            State = "Success",
            TargetInfo = "Test Target",
        };

        var json = MicroJson.Serialize(message);

        var result = MicroJson.Deserialize<TestResultMessage>(json);
    }

}
