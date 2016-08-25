using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace _2DEngine
{
    /// <summary>
    /// A custom implementation of the XNA IntermediateSerializer which can be used to deserialize XML files into the type they represent
    /// </summary>
    public static class XmlDataSerializer
    {
        private static Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

        /// <summary>
        /// Deserialize the xml file represented by the inputted path and return it casted to the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlFilePath"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlFilePath)
        {
            // Create and load the XML document.
            XDocument doc = null;
            try
            {
                doc = XDocument.Load(xmlFilePath);
            }
            catch
            {
                DebugUtils.Fail("There was a problem loading the XML file with path " + xmlFilePath);
                return default(T);
            }

            // Create an XmlReader using the XML document.
            XmlReader nodeReader = doc.CreateReader();

            // Move to the XnaContent element - this is to support backwards compatibility with our existing data files
            // One day we may run a script to change them all
            Debug.Assert(nodeReader.ReadToDescendant("XnaContent"), "No XnaContent element found as root tag");

            // Now move to the Asset element - this contains the name of the type we are trying to load
            Debug.Assert(nodeReader.ReadToDescendant("Asset"), "No Asset element found");

            // Obtain the Type attribute so we can reflectively work out the type
            string dataType = nodeReader.GetAttribute("Type");

            // Get the assembly for the type we are trying to load - this has to pass
            Assembly assembly;
            Debug.Assert(TryGetAssembly(ref dataType, out assembly));

            // Move either to the </Asset> tag if we have no data to load, or to the first data element
            nodeReader.Read();

            // Load the type
            return ReadType<T>(nodeReader, assembly, dataType);
        }

        /// <summary>
        /// Reads the element value of the xml element the inputted reader is currently on as the inputted type.
        /// This method may call itself recursively for a nested list for example.
        /// Finally, returns the successfully deserialized object.
        /// Moves the nodereader past the end tag so do not need to do ReadEndElement after calling this function.
        /// </summary>
        /// <param name="nodeReader"></param>
        /// <param name="typeOfElement"></param>
        /// <returns></returns>
        private static object ReadElementValue(XmlReader nodeReader, Type typeOfElement)
        {
            // Set the value to the default value of the type - for value types we create an instance, for reference types we use null
            object value = null;
            try
            {
                Activator.CreateInstance(typeOfElement);
            }
            catch { }

            if (nodeReader.NodeType == XmlNodeType.Text)
            {
                // Read the element's content and convert it to the type of the property we are setting
                value = nodeReader.ReadContentAs(typeOfElement, null);
            }
            else if (nodeReader.NodeType == XmlNodeType.Element)
            {
                int startingDepth = nodeReader.Depth;

                // Currently only nesting Elements inside other elements is supported
                // This will be called if we are deserializing a list or custom data type
                Debug.Assert(nodeReader.NodeType == XmlNodeType.Element);

                if (IsListType(typeOfElement))
                {
                    value = Activator.CreateInstance(typeOfElement);

                    // Get the add method so we can add objects to the list at runtime
                    MethodInfo addMethod = value.GetType().GetRuntimeMethods().First(x => x.Name == "Add");
                    DebugUtils.AssertNotNull(addMethod);

                    // Keep reading whilst we still have nested elements
                    while (nodeReader.Depth >= startingDepth)
                    {
                        Debug.Assert(typeOfElement.GenericTypeArguments.Length == 1);
                        Type listGenericArgumentType = typeOfElement.GenericTypeArguments[0];
                        object nestedValue = nodeReader.ReadElementContentAs(listGenericArgumentType, null);

                        addMethod.Invoke(value, new object[] { nestedValue });
                    }
                }
                else
                {
                    // We are trying to read a class
                    value = ReadType<object>(nodeReader, typeOfElement);
                }
            }
            else if (nodeReader.NodeType == XmlNodeType.EndElement)
            {
                // This can happen if we have something like an empty string e.g. <String></String>
                // Noop
            }

            // Finally read the end tag
            nodeReader.ReadEndElement();

            return value;
        }

        /// <summary>
        /// Utility function wrapping around ReadType<> where we find the type using the assembly and typeName
        /// </summary>
        /// <param name="nodeReader"></param>
        /// <param name="typeName"></param>
        private static T ReadType<T>(XmlReader nodeReader, Assembly assembly, string typeName)
        {
            Type typeToLoad = assembly.ExportedTypes.FirstOrDefault(x => x.Name == typeName);
            DebugUtils.AssertNotNull(typeToLoad, "Type " + typeName + " specified for data file could not be loaded");

            return ReadType<T>(nodeReader, typeToLoad);
        }

        /// /// <summary>
        /// Deserializes the type with the inputted typename from the inputted assembly using the inputted node reader and returns it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodeReader"></param>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static T ReadType<T>(XmlReader nodeReader, Type typeToLoad)
        {
            string typeName = typeToLoad.Name;

            // Get all the properties for the type we are loading
            List<PropertyInfo> typeProperties = typeToLoad.GetRuntimeProperties().ToList();

            T data = (T)Activator.CreateInstance(typeToLoad);

            // Continuously digest the XML data until we reach the closing Asset tag
            while (nodeReader.NodeType != XmlNodeType.EndElement)
            {
                // Get the name of the property on the data type we are setting
                string propertyName = nodeReader.Name;

                // If we have a value, set it
                if (!nodeReader.IsEmptyElement)
                {
                    // Read the start element so we can then inspect the type of element we are now on
                    nodeReader.ReadStartElement();

                    Debug.Assert(typeProperties.Exists(x => x.Name == propertyName), "No property on data " + typeName + " with with the name " + propertyName);
                    PropertyInfo propertyToSet = typeProperties.Find(x => x.Name == propertyName);

                    object value = ReadElementValue(nodeReader, propertyToSet.PropertyType);

                    // Finally, set the value of the property
                    propertyToSet.SetValue(data, value);
                }
                // Otherwise, just move over this tag and use the default value from the compiler set from the constructor
                else
                {
                    nodeReader.Read();
                }
            }

            return data;
        }

        /// <summary>
        /// A utility function to determine whether the inputted typename is either assembly qualified (Assembly.Type) so we can load the assembly,
        /// or is exported from an assembly already loaded.
        /// Returns true if we can find the assembly for the type, returns false if we cannot (and sets assembly = null).
        /// If the inputted typeName has the correct assembly as a prefix, this will always pass.
        /// It also strips the assembly name from the inputted typeName if it exists.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static bool TryGetAssembly(ref string typeName, out Assembly assembly)
        {
            // Our type is going to be of the form 'Assembly.Rest'
            int firstDotIndex = typeName.IndexOf('.');

            // '.' cannot be the first element in the string
            if (firstDotIndex > 0)
            {
                // We have the formatting for Assembly.Type so try extracting the assembly name and loading
                string assemblyName = typeName.Substring(0, firstDotIndex);
                typeName = typeName.Substring(firstDotIndex + 1);

                // See if we have already loaded the assembly
                if (!Assemblies.TryGetValue(assemblyName, out assembly))
                {
                    assembly = Assembly.Load(new AssemblyName(assemblyName));

                    DebugUtils.AssertNotNull(assembly);
                    Assemblies.Add(assemblyName, assembly);
                }

                return true;
            }
            else
            {
                // For lambda - cannot use refs inside them
                string type = typeName;

                // No assembly name was given so try and load it from the ones we have already
                foreach (Assembly ass in Assemblies.Values)
                {
                    if (ass.ExportedTypes.ToList().Exists(x => x.Name == type))
                    {
                        assembly = ass;
                        return true;
                    }
                }
            }

            assembly = null;
            return false;
        }

        /// <summary>
        /// Utility function for determining if a type represents a list
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsListType(Type type)
        {
            return type.IsConstructedGenericType;// &&
            //       type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        /// <summary>
        /// Serializes the inputted data and saves it to an xml file with the inputted path
        /// </summary>
        /// <param name="xmlFilePath"></param>
        public static void Serialize<T>(T data, string xmlFilePath)
        {
            //XmlWriterSettings settings = new XmlWriterSettings();
            //settings.Indent = true;

            //Directory.CreateDirectory(Path.GetDirectoryName(xmlFilePath));
            //using (XmlWriter writer = XmlWriter.Create(xmlFilePath, settings))
            //{

            //}
        }
    }
}
