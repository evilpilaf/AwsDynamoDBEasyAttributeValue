using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
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
        public void ObjectAttributesAreMappedToMAttribute()
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
    }
}
