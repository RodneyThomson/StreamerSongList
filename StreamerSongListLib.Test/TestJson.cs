using StreamerSongList.Datatypes;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamerSongListLib.Test
{
    [TestClass]
    public sealed class TestJson
    {
        [TestMethod]
        public void TestNullableSerialise()
        {
            // This test verifies that the JSON serialisation does not include properties that are null when using DefaultIgnoreCondition = WhenWritingNull
            var defaultSong = new UpdateSongBody();
            var json = JsonSerializer.Serialize(defaultSong, new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

            // Expect an empty but valid JSON object since all properties are null
            Assert.IsNotNull(json);
            Assert.DoesNotContain(nameof(UpdateSongBody.AttributeIds), json);
            Assert.DoesNotContain(nameof(UpdateSongBody.MinAmount), json);

            // This should successfully deserialise back to an object with null properties
            var deserialisedSong = JsonSerializer.Deserialize<UpdateSongBody>(json);
            Assert.IsNotNull(deserialisedSong);
            Assert.IsNull(deserialisedSong.AttributeIds);
            Assert.IsNull(deserialisedSong.MinAmount);

            // Set the MinAmount, verify it appears in the JSON output, but not the AttributeIds
            defaultSong.MinAmount = 5;
            json = JsonSerializer.Serialize(defaultSong, new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            Assert.DoesNotContain(nameof(UpdateSongBody.AttributeIds), json);
            Assert.Contains(nameof(UpdateSongBody.MinAmount), json);

            // This should successfully deserialise back to an object with null AttributeIds and a non-null MinAmount
            deserialisedSong = JsonSerializer.Deserialize<UpdateSongBody>(json);
            Assert.IsNotNull(deserialisedSong);
            Assert.IsNull(deserialisedSong.AttributeIds);
            Assert.IsNotNull(deserialisedSong.MinAmount);

            // Set the AttributeIds, verify it too now appears in the JSON output
            defaultSong.AttributeIds = new List<int> { 1, 2, 3 };
            json = JsonSerializer.Serialize(defaultSong, new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            Assert.Contains(nameof(UpdateSongBody.MinAmount), json);
            Assert.Contains(nameof(UpdateSongBody.AttributeIds), json);

            // This should successfully deserialise back to an object with non-null AttributeIds and MinAmount
            deserialisedSong = JsonSerializer.Deserialize<UpdateSongBody>(json);
            Assert.IsNotNull(deserialisedSong);
            Assert.IsNotNull(deserialisedSong.AttributeIds);
            Assert.IsNotNull(deserialisedSong.MinAmount);
        }
    }
}