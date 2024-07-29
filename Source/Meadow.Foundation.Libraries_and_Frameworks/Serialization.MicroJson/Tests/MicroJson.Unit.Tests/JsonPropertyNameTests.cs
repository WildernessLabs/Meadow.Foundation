using Meadow.Foundation.Serialization;
using Xunit;

namespace Unit.Tests;

public class JsonPropertyNameTests
{
    [Fact]
    public void SerializePropertyNameTest()
    {
        var input = new RenamedPropertyClass
        {
            Name = "property value",
            OtherProp = "some other prop"
        };

        var json = MicroJson.Serialize(input);

        Assert.NotNull(json);
        Assert.Contains("\"prop_name\":", json);
    }

    [Fact]
    public void DeserializePropertyNameTest()
    {
        var json = "{ \"prop_name\":\"property value\" }";
        var test = MicroJson.Deserialize<RenamedPropertyClass>(json);

        Assert.NotNull(test);

    }

    [Fact]
    public void DeserializeAuthResponseMessageTest()
    {
        var json = "{\"token_type\":\"Bearer\",\"expires_in\":3600,\"access_token\":\"eyJraWQiOiJCbnFGa1BnVGF3eF9zVVg5ZlFSRm85WEFvamtSdjNiLVpuMXhjMTl4aEgwIiwiYWxnIjoiUlMyNTYifQ.eyJ2ZXIiOjEsImp0aSI6IkFULkhaRm1TSGw5cXZQeDdtcHhzRkNQRFhFYlVqcHoySXBtdGY1TFctT3RsUzAiLCJpc3MiOiJodHRwczovL2lkZW50aXR5LndpbGRlcm5lc3NsYWJzLmNvL29hdXRoMi9kZWZhdWx0IiwiYXVkIjoiYXBpOi8vZGVmYXVsdCIsImlhdCI6MTcxOTYwNDI2NSwiZXhwIjoxNzE5NjA3ODY1LCJjaWQiOiIwb2FncDJyNmllWUxua3hzejVkNyIsInNjcCI6WyJzYW1wbGVfc2NvcGUiXSwic3ViIjoiMG9hZ3AycjZpZVlMbmt4c3o1ZDcifQ.eUbAfuFXNp3TkIKuN4ZCzS-9DyXCydv8eRymB5VdLQn942xADCQoao7CO94n9kfXj2yQkayins-yTZx8r_I97tXg97npUE57uP1IJ8Nxc1SnfsW0zzRFvDZTTqk-_z8OXOIBE1fnoY42a2qzh2-NE3IxgHlgxLo50gHqJA3yex3TrUw3Pup9ZydlEOKJcNxkvKtMdztSR8tzyo-8Irn6p8_KLEH87eQy3KDxtU6G9hxJ70A9DLmMvkwL1Y7vW1bKfdB_qYLpuIw8O7g8h86rNxvw_E1ewpZL4mV-UCt5_T6iJn0R_I-sscDrZOq1nnWjtnLChVFxFFnntolWNWtcaQ\",\"scope\":\"sample_scope\"}";
        var test = MicroJson.Deserialize<AuthResponseMessage>(json);

        Assert.NotNull(test);

    }
}
