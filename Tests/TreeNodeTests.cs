using FluentAssertions;
using Netryoshka;
using Newtonsoft.Json;
using System.Text;
using Xunit;
using static Netryoshka.TSharkTls;

namespace Tests
{
    public class TreeNodeTests
    {
        public static string PrintTreeNode(TreeNode node, int nSpaces = 0)
        {
            var result = !string.IsNullOrEmpty(node.PropertyValue)
                         ? $"{node.PropertyName}:{node.PropertyValue}"
                         : $"{node.PropertyName}";

            if (node.Children?.Any() == true)
            {
                result = $"> {result}";
                foreach (var child in node.Children)
                {
                    result += Environment.NewLine + new string(' ', nSpaces + 2) + PrintTreeNode(child, nSpaces + 2);
                }
            }

            return result;
        }


        public static string PrintTreeNodes(List<TreeNode> nodes, int nSpaces = 0)
        {
            var sb = new StringBuilder();
            foreach (var node in nodes)
            {
                sb.AppendLine(PrintTreeNode(node, nSpaces));
            }
            return sb.ToString().TrimEnd();
        }


        [Fact]
        public void BuildFromObject_ShouldCreateExpectedTreeNode()
        {
            // Arrange
            var tlsTestClass = new TSharkTls
            {
                Records = new List<TlsRecord>
                {
                    new TlsRecord {
                        ContentType = "22",
                        Version = "0x0303",
                        Handshake = new List<TlsHandshake> { new TlsHandshake { HandshakeType = "2" } }
                    },
                    new TlsRecord { ContentType = "20",
                        Version = "0x0303"
                    }
                }
            };

            // Act
            var treeNodes = TreeNode.BuildNodesFromObject(tlsTestClass).First();

            // Assert
            var expectedOutput = @"> 
  > tls.record[0]
    tls.record.content_type:""22""
    tls.record.version:""0x0303""
    > tls.handshake[0]
      tls.handshake.type:""2""
  > tls.record[1]
    tls.record.content_type:""20""
    tls.record.version:""0x0303""";

            var actualOutput = PrintTreeNode(treeNodes);
            actualOutput.Should().Be(expectedOutput);
        }


        [Fact]
        public void BuildFromObject_ShouldCreateExpectedTreeNode2()
        {
            // Arrange
            var tlsTestClass = new TSharkTls
            {
                Records = new List<TlsRecord>
                {
                    new TlsRecord {
                        ContentType = "22",
                        Version = "0x0303",
                        Handshake = new List<TlsHandshake> { new TlsHandshake { HandshakeType = "2" } }
                    },
                    new TlsRecord { ContentType = "20",
                        Version = "0x0303"
                    }
                }
            };

            // Act
            var treeNodes = TreeNode.BuildNodesFromObject(tlsTestClass);

            // Assert
            var expectedNodes = new List<TreeNode>
            {
                new TreeNode
                {
                    PropertyName = "",
                    Children = new List<TreeNode>
                    {
                        new TreeNode
                        {
                            PropertyName = "tls.record[0]",
                            Children = new List<TreeNode>
                            {
                                new TreeNode { PropertyName = "tls.record.content_type", PropertyValue = "\"22\"" },
                                new TreeNode { PropertyName = "tls.record.version", PropertyValue = "\"0x0303\"" },
                                new TreeNode
                                {
                                    PropertyName = "tls.handshake[0]",
                                    Children = new List<TreeNode>
                                    {
                                        new TreeNode { PropertyName = "tls.handshake.type", PropertyValue = "\"2\"" },
                                    }
                                }
                            }
                        },
                        new TreeNode
                        {
                            PropertyName = "tls.record[1]",
                            Children = new List<TreeNode>
                            {
                                new TreeNode { PropertyName = "tls.record.content_type", PropertyValue = "\"20\"" },
                                new TreeNode { PropertyName = "tls.record.version", PropertyValue = "\"0x0303\"" }
                            }
                        }
                    }
                }
            };

            AssertionOptions.FormattingOptions.MaxDepth = 10;
            treeNodes.Should().BeEquivalentTo(expectedNodes);
        }


        public class Person
        {
            [JsonProperty("name")] public string? Name { get; set; }
            [JsonProperty("age")] public int? Age { get; set; }
            [JsonProperty("isEmployed")] public bool? IsEmployed { get; set; }
            [JsonProperty("address")] public Address? HomeAddress { get; set; }
            [JsonProperty("phoneNumbers")] public List<string>? PhoneNumbers { get; set; }

            public class Address
            {
                [JsonProperty("street")] public string? Street { get; set; }
                [JsonProperty("city")] public string? City { get; set; }
                [JsonProperty("state")] public string? State { get; set; }
                [JsonProperty("zip")] public string? Zip { get; set; }
            }
        }
        

        [Fact]
        public void BuildNodesFromJson_ShouldCreateExpectedTreeStructure()
        {
            // Arrange
            var json = @"
            [
                {
                    ""name"": ""Alice"",
                    ""age"": 30,
                    ""isEmployed"": true,
                    ""address"": {
                        ""street"": ""123 Main St"",
                        ""city"": ""Springfield"",
                        ""state"": ""IL"",
                        ""zip"": ""62704""
                    },
                    ""phoneNumbers"": [""555-1234"", ""555-5678""]
                },
                {
                    ""name"": ""Bob"",
                    ""age"": 40,
                    ""isEmployed"": false,
                    ""address"": null,
                    ""phoneNumbers"": []
                }
            ]";

            // Act
            var treeNodes = TreeNode.BuildNodesFromJson(json, "person");

            // Assert
            var expectedOutput = @"> person[0]
  name:""Alice""
  age:30
  isEmployed:True
  > address
    street:""123 Main St""
    city:""Springfield""
    state:""IL""
    zip:""62704""
  phoneNumbers[0]:""555-1234""
  phoneNumbers[1]:""555-5678""
> person[1]
  name:""Bob""
  age:40
  isEmployed:False
  address";

            var actualOutput = PrintTreeNodes(treeNodes);
            actualOutput.Should().Be(expectedOutput);
        }


        [Fact]
        public void BuildNodesFromJson_ShouldIncludeRootNameForSingleObject()
        {
            // Arrange
            var json = @"{ ""name"": ""Alice"" }";

            // Act
            var treeNodes = TreeNode.BuildNodesFromJson(json, "person");

            // Assert
            var expectedOutput = @"> person
  name:""Alice""";

            var actualOutput = PrintTreeNodes(treeNodes);
            actualOutput.Should().Be(expectedOutput);
        }


        [Fact]
        public void BuildNodesFromJson_ShouldOmitRootNameWhenNotProvided()
        {
            // Arrange
            var json = @"{ ""name"": ""Alice"" }";

            // Act
            var treeNodes = TreeNode.BuildNodesFromJson(json);

            // Assert
            var expectedOutput = @"> 
  name:""Alice""";

            var actualOutput = PrintTreeNodes(treeNodes);
            actualOutput.Should().Be(expectedOutput);
        }

    }
}

