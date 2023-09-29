using FluentAssertions;
using Netryoshka;
using Newtonsoft.Json;

namespace Tests
{
    public class WireSharkPacketTests
    {
        [Fact]
        public void ShouldDeserializeJsonObjectIntoDictionary()
        {
            // Arrange

            var jsonJson = /*lang=json,strict*/ @"{
  ""json.object"": {
    ""json.member"": ""detail"",
    ""json.member_tree"": {
      ""json.object"": {
        ""json.member"": ""message"",
        ""json.member_tree"": {
          ""json.path_with_value"": ""/detail/message:Your authentication token has expired. Please try signing in again."",
          ""json.member_with_value"": ""message:Your authentication token has expired. Please try signing in again."",
          ""json.value.string"": ""Your authentication token has expired. Please try signing in again."",
          ""json.key"": ""message"",
          ""json.path"": ""/detail/message""
        },
        ""json.member"": ""type"",
        ""json.member_tree"": {
          ""json.path_with_value"": ""/detail/type:invalid_request_error"",
          ""json.member_with_value"": ""type:invalid_request_error"",
          ""json.value.string"": ""invalid_request_error"",
          ""json.key"": ""type"",
          ""json.path"": ""/detail/type""
        },
        ""json.member"": ""param"",
        ""json.member_tree"": {
          ""json.path_with_value"": ""/detail/param:null"",
          ""json.member_with_value"": ""param:null"",
          ""json.value.null"": """",
          ""json.key"": ""param"",
          ""json.path"": ""/detail/param""
        },
        ""json.member"": ""code"",
        ""json.member_tree"": {
          ""json.path_with_value"": ""/detail/code:token_expired"",
          ""json.member_with_value"": ""code:token_expired"",
          ""json.value.string"": ""token_expired"",
          ""json.key"": ""code"",
          ""json.path"": ""/detail/code""
        }
      },
      ""json.key"": ""detail"",
      ""json.path"": ""/detail""
    }
  }
}";

            // Act
            var result = JsonConvert.DeserializeObject<TSharkJson>(jsonJson);

            // Assert
            result.Should().NotBeNull();
            result!.Object.Should().NotBeNull();
            result!.Object.Should().ContainKey("detail");

            var detailMemberTree = result!.Object!["detail"];
            detailMemberTree.Should().NotBeNull();
            detailMemberTree!.Object.Should().NotBeNull();
            detailMemberTree!.Object.Should().ContainKey("message");

            var messageMember = detailMemberTree!.Object!["message"];
            messageMember.Should().NotBeNull();
            messageMember!.ValueString.Should().Be("Your authentication token has expired. Please try signing in again.");
            messageMember!.Key.Should().Be("message");
            messageMember!.Path.Should().Be("/detail/message");
        }

    }
}
