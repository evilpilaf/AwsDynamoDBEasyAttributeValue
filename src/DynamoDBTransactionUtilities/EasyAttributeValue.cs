using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public EasyAttributeValue(AttributeValue instance)
        {
            _value = instance;
        }

        public static EasyAttributeValue FromProperties(Dictionary<string, AttributeValue> values)
        {
            return new EasyAttributeValue(values);
        }

        public static EasyAttributeValue FromCollection<T>(IEnumerable<T> values)
        {
            return new AttributeValue
            {
                L = values.Select(v => (AttributeValue)FromObject(v)).ToList()
            };
        }

        public static EasyAttributeValue FromObject(object obj)
        {
            var objectType = obj.GetType();
            EasyAttributeValue attributeValue;

            if (IsEnumeration(objectType))
            {
                attributeValue = FromClass(obj);
            }
            else if (IsClass(objectType))
            {
                attributeValue = FromClass(obj);
            }
            else
            {
                attributeValue = FromPrimitive(obj);
            }
            return attributeValue;
        }

        private static EasyAttributeValue FromClass<T>(T obj) where T : class
        {
            var classProperties = new Dictionary<string, AttributeValue>();
            var objectProperties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in objectProperties)
            {
                var value = property.GetValue(obj);
                EasyAttributeValue attributeValue;

                var type = value.GetType();

                if (IsEnumeration(type))
                {
                    attributeValue = FromCollection(value as IEnumerable<object>);
                }
                else if (IsClass(type))
                {
                    attributeValue = FromObject(value);
                }
                else
                {
                    attributeValue = FromPrimitive(value);
                }
                classProperties.Add(property.Name, attributeValue);
            }
            return new EasyAttributeValue(classProperties);
        }

        public static implicit operator AttributeValue(EasyAttributeValue instance)
        {
            return instance._value;
        }

        public static implicit operator EasyAttributeValue(AttributeValue instance)
        {
            return new EasyAttributeValue(instance);
        }

        private static EasyAttributeValue FromPrimitive(object value)
        {
            EasyAttributeValue attributeValue;
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

            return attributeValue;
        }

        private static bool IsClass(Type type) =>
            type.IsClass && type != typeof(string);

        private static bool IsEnumeration(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
    }
}
