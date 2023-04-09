using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers
{
    public static class IEnumerableExtensions
    {
        //works on generic type TSource and returns an Ienumerable of ExpandoObject
        public static IEnumerable<ExpandoObject> ShapeData<TSource>(this IEnumerable<TSource> source, string? fields)
        {
            if(source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // Create a lst to hold our expandoObjects
            var expandoObjectList = new List<ExpandoObject>();

            // Create a list with PropertyInfo objects on TSource. Reflection is
            // expensive, so rather than doing it for each object in the list, we
            // do it once and reuse the results. After all, part of the reflection is on type of the object (TSource), not on the instance.

            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // all public properties should be in the expando object
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);


            }
            else
            {
                // the fields are seperated by ",", so we split it.
                var fieldsAfterSplit = fields.Split(',');

                foreach(var field in fieldsAfterSplit)
                {
                    // trim each field, as it might contain leading
                    // or trailing spaces. Can't trim the var in foreach.
                    // so use another var

                    var propertyName = field.Trim();

                    // Use reflection to get the property on the source object
                    //We need to include public and instance, b/c specifying a 
                    //binding flag overwrites the already existing Binfing flags.

                    var propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if(propertyInfo == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
                    }

                    // add propertyInfo to list
                    propertyInfoList.Add(propertyInfo);

                }
            }

            //run through the source objects

            foreach(TSource sourceObject in source)
            {
                // Create an ExpandoObject that will hold the
                // selected properties and values

                var dataShapedObject = new ExpandoObject();

                // Get the value of each property  we have to return. For that,
                // We run through the list

                foreach(var propertyInfo in propertyInfoList)
                {
                    // Get value returns the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(sourceObject);

                    // add the field to the ExpandoObject
                    ((IDictionary<string,object?>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }

                // Add the expandoObject to the list
                expandoObjectList.Add(dataShapedObject);
            }

            //return the list
            return expandoObjectList;
        }
    }
}
