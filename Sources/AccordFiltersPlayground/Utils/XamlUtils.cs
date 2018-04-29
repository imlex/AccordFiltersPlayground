using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;

namespace AccordFiltersPlayground.Utils
{
    public static class XamlUtils
    {
        private class BindingConvertor : ExpressionConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(MarkupExtension);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(MarkupExtension))
                    return value is BindingExpression bindingExpression ? bindingExpression.ParentBinding : throw new ArgumentOutOfRangeException(nameof(value));

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        static XamlUtils()
        {
            TypeDescriptor.AddAttributes(typeof(BindingExpression), new TypeConverterAttribute(typeof(BindingConvertor)));
        }

        public static string ToXaml<T>(T obj)
        {
            StringBuilder xamlStringBuilder = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(xamlStringBuilder, new XmlWriterSettings {OmitXmlDeclaration = true}))
            {
                XamlDesignerSerializationManager xamlDesignerSerializationManager = new XamlDesignerSerializationManager(xmlWriter) {XamlWriterMode = XamlWriterMode.Expression};
                XamlWriter.Save(obj, xamlDesignerSerializationManager);
            }

            return xamlStringBuilder.ToString();
        }

        public static T FromXaml<T>(string xaml)
        {
            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
                return (T) XamlReader.Load(memoryStream);
        }
    }
}