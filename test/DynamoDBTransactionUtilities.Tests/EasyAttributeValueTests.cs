using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DynamoDBTransactionUtilities.Tests
{
    public sealed class EasyAttributeValueTests
    {
        [Fact]
        public void IntValuesAreMappedToNumberType()
        {
            const int value = 4;
            var sut = new EasyAttributeValue(value);

            AttributeValue projection = sut;

            projection.N.Should().Be(value.ToString());
        }

        [Fact]
        public void DoubleValuesAreMappedToNumberType()
        {
            const double value = 4.0;
            var sut = new EasyAttributeValue(value);

            AttributeValue projection = sut;

            projection.N.Should().Be(value.ToString());
        }

        [Fact]
        public void FloatValuesAreMappedToNumberType()
        {
            const float value = 4.0F;
            var sut = new EasyAttributeValue(value);

            AttributeValue projection = sut;

            projection.N.Should().Be(value.ToString());
        }

        [Fact]
        public void DecimalValuesAreMappedToNumberType()
        {
            const decimal value = 4.0m;
            var sut = new EasyAttributeValue(value);

            AttributeValue projection = sut;

            projection.N.Should().Be(value.ToString());
        }

        [Fact]
        public void StringValuesAreMappedToStringType()
        {
            const string value = nameof(value);
            var sut = new EasyAttributeValue(value);

            AttributeValue projection = sut;

            projection.S.Should().Be(value);
        }

        [Fact]
        public void NullStringsValuesAreMappedToStringType()
        {
            const string value = null;
            var sut = new EasyAttributeValue(value);

            AttributeValue projection = sut;

            projection.S.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BoolValuesAreMappedToBooleanType(bool value)
        {
            var sut = new EasyAttributeValue(value);

            AttributeValue projection = sut;

            projection.BOOL.Should().Be(value);
        }

        [Fact]
        public void DateTimeValuesAreMappedToISODateString()
        {
            var value = DateTime.UtcNow;

            var sut = new EasyAttributeValue(value);

            AttributeValue projection = sut;

            projection.S.Should().Be(value.ToString("o"));
        }

        [Fact]
        public void DateTimeOffsetValuesAreMappedToISODateString()
        {
            var value = DateTimeOffset.UtcNow;

            var sut = new EasyAttributeValue(value);

            AttributeValue projection = sut;

            projection.S.Should().Be(value.ToString("o"));
        }

        [Fact]
        public void AttributesAreMappedToMAttribute()
        {
            dynamic value = new
            {
                NumericValue = 1,
                StringValue = "StringValue"
            };

            var values = new Dictionary<string, AttributeValue>
            {
                { nameof(value.NumericValue), new EasyAttributeValue(value.NumericValue) },
                { nameof(value.StringValue), new EasyAttributeValue(value.StringValue) }
            };

            var sut = EasyAttributeValue.FromProperties(values);

            AttributeValue projection = sut;

            using (new AssertionScope())
            {
                projection.IsMSet.Should().BeTrue();
                projection.M.Should().NotBeNull();
                projection.M.Should().BeEquivalentTo(values);
            }
        }

        [Fact]
        public void Object_WithPrimitiveProperties_AreMappedToMAttribute()
        {
            dynamic value = new
            {
                NumericValue = 1,
                StringValue = "StringValue",
                BooleanValue = true,
                DateTimeValue = DateTime.Now,
                DateTimeOffsetValue = DateTimeOffset.Now
            };

            var sut = EasyAttributeValue.FromObject(value);

            AttributeValue projection = sut;

            using (new AssertionScope())
            {
                projection.IsMSet.Should().BeTrue();
                projection.M.Should().NotBeNull();

                projection.M.Should().ContainKey(nameof(value.NumericValue));
                projection.M[nameof(value.NumericValue)].Should().BeEquivalentTo(new AttributeValue { N = value.NumericValue.ToString() });

                projection.M.Should().ContainKey(nameof(value.StringValue));
                projection.M[nameof(value.StringValue)].Should().BeEquivalentTo(new AttributeValue { S = value.StringValue });

                projection.M.Should().ContainKey(nameof(value.BooleanValue));
                projection.M[nameof(value.BooleanValue)].Should().BeEquivalentTo(new AttributeValue { BOOL = value.BooleanValue });

                projection.M.Should().ContainKey(nameof(value.DateTimeValue));
                projection.M[nameof(value.DateTimeValue)].Should().BeEquivalentTo(new AttributeValue { S = value.DateTimeValue.ToString("O") });

                projection.M.Should().ContainKey(nameof(value.DateTimeOffsetValue));
                projection.M[nameof(value.DateTimeOffsetValue)].Should().BeEquivalentTo(new AttributeValue { S = value.DateTimeOffsetValue.ToString("O") });
            }
        }

        [Fact]
        public void Object_WithNestedObject_MappedToMAttribute()
        {
            var value = new
            {
                NestedValue = new
                {
                    NumericValue = 1,
                    StringValue = "StringValue",
                    BooleanValue = true,
                    DateTimeValue = DateTime.Now,
                    DateTimeOffsetValue = DateTimeOffset.Now
                }
            };

            var sut = EasyAttributeValue.FromObject(value);

            AttributeValue projection = sut;

            using (new AssertionScope())
            {
                projection.IsMSet.Should().BeTrue();
                projection.M.Should().NotBeNull();

                projection.M.Should().ContainKey(nameof(value.NestedValue));

                var nestedAttribute = projection.M[nameof(value.NestedValue)];

                nestedAttribute.M.Should().ContainKey(nameof(value.NestedValue.NumericValue));
                nestedAttribute.M[nameof(value.NestedValue.NumericValue)].Should().BeEquivalentTo(new AttributeValue { N = value.NestedValue.NumericValue.ToString() });

                nestedAttribute.M.Should().ContainKey(nameof(value.NestedValue.StringValue));
                nestedAttribute.M[nameof(value.NestedValue.StringValue)].Should().BeEquivalentTo(new AttributeValue { S = value.NestedValue.StringValue });

                nestedAttribute.M.Should().ContainKey(nameof(value.NestedValue.BooleanValue));
                nestedAttribute.M[nameof(value.NestedValue.BooleanValue)].Should().BeEquivalentTo(new AttributeValue { BOOL = value.NestedValue.BooleanValue });

                nestedAttribute.M.Should().ContainKey(nameof(value.NestedValue.DateTimeValue));
                nestedAttribute.M[nameof(value.NestedValue.DateTimeValue)].Should().BeEquivalentTo(new AttributeValue { S = value.NestedValue.DateTimeValue.ToString("O") });

                nestedAttribute.M.Should().ContainKey(nameof(value.NestedValue.DateTimeOffsetValue));
                nestedAttribute.M[nameof(value.NestedValue.DateTimeOffsetValue)].Should().BeEquivalentTo(new AttributeValue { S = value.NestedValue.DateTimeOffsetValue.ToString("O") });
            }
        }

        [Fact]
        public void Collection_OfPrimitiveValues_MapsToLAttribute()
        {
            var lst = Enumerable.Range(1, 2);

            AttributeValue sut = EasyAttributeValue.FromCollection(lst);

            var expectedValues = lst.Select(v => new AttributeValue { N = v.ToString() });
            using (new AssertionScope())
            {
                sut.L.Should().HaveCount(lst.Count());
                sut.L.Should().BeEquivalentTo(expectedValues);
            }
        }

        [Fact]
        public void Collection_OfObjects_MapsToLAttribute()
        {
            var lst = new[] { new { MyValue = "Value" } };

            AttributeValue sut = EasyAttributeValue.FromCollection(lst);

            var expectedValue = new[]
            {
                new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        { "MyValue", new AttributeValue { S = "Value" } }
                    }
                }
            };

            using (new AssertionScope())
            {
                sut.L.Should().HaveCount(lst.Count());
                sut.L.Should().BeEquivalentTo(expectedValue);
            }
        }
    }
}