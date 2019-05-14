using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Reflection;

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
            _value = new AttributeValue { N = value.ToString() };
        }

        public EasyAttributeValue(float value)
        {
            _value = new AttributeValue { N = value.ToString() };
        }

        public EasyAttributeValue(double value)
        {
            _value = new AttributeValue { N = value.ToString() };
        }

        public EasyAttributeValue(decimal value)
        {
            _value = new AttributeValue { N = value.ToString() };
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

        internal EasyAttributeValue(Dictionary<string, AttributeValue> values)
        {
            _value = new AttributeValue { M = values };
        }

        public static EasyAttributeValue FromProperties(Dictionary<string, AttributeValue> values)
        {
            return new EasyAttributeValue(values);
        }

        public static EasyAttributeValue FromObject<T>(T obj) where T : class
        {
            var classProperties = new Dictionary<string, AttributeValue>();
            var objectProperties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in objectProperties)
            {
                var value = property.GetValue(obj);
                EasyAttributeValue attributeValue;

                var type = value.GetType();

                if (type.IsClass && type != typeof(string))
                {
                    attributeValue = FromObject(value);
                }
                else
                {
                    switch (value)
                    {
                        case int v:
                            attributeValue = new EasyAttributeValue(v);
                            break;
                        case float v:
                            attributeValue = new EasyAttributeValue(v);
                            break;
                        case double v:
                            attributeValue = new EasyAttributeValue(v);
                            break;
                        case string v:
                            attributeValue = new EasyAttributeValue(v);
                            break;
                        case decimal v:
                            attributeValue = new EasyAttributeValue(v);
                            break;
                        case bool v:
                            attributeValue = new EasyAttributeValue(v);
                            break;
                        case DateTime v:
                            attributeValue = new EasyAttributeValue(v);
                            break;
                        case DateTimeOffset v:
                            attributeValue = new EasyAttributeValue(v);
                            break;
                        default:
                            throw new Exception();
                    }
                }
                classProperties.Add(property.Name, attributeValue);
            }
            return new EasyAttributeValue(classProperties);
        }

        public static implicit operator AttributeValue(EasyAttributeValue instance)
        {
            return instance._value;
        }
    }
}
