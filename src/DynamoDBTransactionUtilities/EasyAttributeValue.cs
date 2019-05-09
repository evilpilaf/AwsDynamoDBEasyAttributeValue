using Amazon.DynamoDBv2.Model;
using System;

namespace DynamoDBTransactionUtilities
{
    public readonly struct EasyAttributeValue
    {
        private readonly AttributeValue _value;

        public EasyAttributeValue(string value)
        {
            _value = new AttributeValue { S = value };
        }

        public EasyAttributeValue(int value)
        {
            _value = new AttributeValue { N = $"{value}" };
        }

        public EasyAttributeValue(float value)
        {
            _value = new AttributeValue { N = $"{value}" };
        }

        public EasyAttributeValue(double value)
        {
            _value = new AttributeValue { N = $"{value}" };
        }

        public EasyAttributeValue(decimal value)
        {
            _value = new AttributeValue { N = $"{value}" };
        }

        public EasyAttributeValue(DateTime value)
        {
            _value = new AttributeValue { S = value.ToString("O") };
        }

        public EasyAttributeValue(DateTimeOffset value)
        {
            _value = new AttributeValue { S = value.ToString("O") };
        }

        public EasyAttributeValue(bool value)
        {
            _value = new AttributeValue { BOOL = value };
        }

        public static implicit operator AttributeValue(EasyAttributeValue instance)
        {
            return instance._value;
        }
    }
}
