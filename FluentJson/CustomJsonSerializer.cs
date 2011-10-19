using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Collections;
using System.Reflection;
using System.Globalization;
using System.Web;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;

namespace FluentJson
{
    class CustomJsonSerializer
    {
        static readonly long DatetimeMinTimeTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        const int DefaultRecursionLimit = 100;
        const int DefaultMaxJsonLength = 2097152;
        
        internal string Serialize(object obj)
        {
            var output = new StringBuilder();

            Serialize(obj, output);

            return ((object)output).ToString();
        }

        internal void Serialize(object obj, StringBuilder output)
        {
            SerializeValue(obj, output, 0, (Hashtable)null);

            if (output.Length > DefaultMaxJsonLength)
                throw new InvalidOperationException();
        }

        private static void SerializeBoolean(bool o, StringBuilder sb)
        {
            if (o)
                sb.Append("true");
            else
                sb.Append("false");
        }

        private static void SerializeUri(Uri uri, StringBuilder sb)
        {
            sb.Append("\"").Append(uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped)).Append("\"");
        }

        private static void SerializeGuid(Guid guid, StringBuilder sb)
        {
            sb.Append("\"").Append(guid.ToString()).Append("\"");
        }

        private static void SerializeDateTime(DateTime datetime, StringBuilder sb)
        {
            //if (serializationFormat == SerializationFormat.JSON)
            //{
                sb.Append("\"\\/Date(");
                sb.Append((datetime.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000L);
                sb.Append(")\\/\"");
            //}
            //else
            //{
            //    sb.Append("new Date(");
            //    sb.Append((datetime.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000L);
            //    sb.Append(")");
            //}
        }

        private void SerializeCustomObject(object o, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            var flag = true;
            var type = o.GetType();

            sb.Append('{');

            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!field.IsDefined(typeof(ScriptIgnoreAttribute), true))
                {
                    if (!flag)
                        sb.Append(',');

                    SerializeString(field.Name, sb);

                    sb.Append(':');

                    this.SerializeValue(FieldInfoGetValue(field, o), sb, depth, objectsInUse);

                    flag = false;
                }
            }

            foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty))
            {
                if (!propertyInfo.IsDefined(typeof(ScriptIgnoreAttribute), true))
                {
                    var getMethod = propertyInfo.GetGetMethod();

                    if (!(getMethod == (MethodInfo)null) && getMethod.GetParameters().Length <= 0)
                    {
                        if (!flag)
                            sb.Append(',');

                        SerializeString(propertyInfo.Name, sb);

                        sb.Append(':');

                        this.SerializeValue(MethodInfoInvoke(getMethod, o, (object[])null), sb, depth, objectsInUse);

                        flag = false;
                    }
                }
            }

            sb.Append('}');
        }

        private void SerializeDictionary(IDictionary o, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            sb.Append('{');

            var flag1 = true;

            foreach (DictionaryEntry dictionaryEntry in o)
            {
                var str = dictionaryEntry.Key as string;
                if (str == null)
                    throw new ArgumentException("Null dictionary key", "o");
                else
                {
                    if (!flag1)
                        sb.Append(',');

                    this.SerializeDictionaryKeyValue(str, dictionaryEntry.Value, sb, depth, objectsInUse);

                    flag1 = false;
                }
            }

            sb.Append('}');
        }

        private void SerializeDictionaryKeyValue(string key, object value, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            SerializeString(key, sb);

            sb.Append(':');

            this.SerializeValue(value, sb, depth, objectsInUse);
        }

        private void SerializeEnumerable(IEnumerable enumerable, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            sb.Append('[');

            var flag = true;

            foreach (object o in enumerable)
            {
                if (!flag)
                    sb.Append(',');
                this.SerializeValue(o, sb, depth, objectsInUse);
                flag = false;
            }

            sb.Append(']');
        }

        private static void SerializeString(string input, StringBuilder sb)
        {
            sb.Append('"');
            sb.Append(HttpUtility.JavaScriptStringEncode(input));
            sb.Append('"');
        }

        private void SerializeValue(object o, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            if (++depth > DefaultRecursionLimit)
                throw new ArgumentOutOfRangeException("depth");

            this.SerializeValueInternal(o, sb, depth, objectsInUse);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void SerializeValueInternal(object o, StringBuilder sb, int depth, Hashtable objectsInUse)
        {
            if (o == null || DBNull.Value.Equals(o))
            {
                sb.Append("null");
            }
            else
            {
                var input = o as string;
                var html = o as IHtmlString;

                if (input != null)
                    SerializeString(input, sb);
                else if (o is IJsonSerializable)
                {
                    var js = o as IJsonSerializable;
                    js.BuildJson(sb);
                }
                else if (html != null)
                {
                    SerializeString(html.ToHtmlString(), sb);
                }
                else if (o is char)
                {
                    if ((int)(char)o == 0)
                        sb.Append("null");
                    else
                        SerializeString(o.ToString(), sb);
                }
                else if (o is bool)
                    SerializeBoolean((bool)o, sb);
                else if (o is DateTime)
                    SerializeDateTime((DateTime)o, sb);
                else if (o is DateTimeOffset)
                    SerializeDateTime(((DateTimeOffset)o).UtcDateTime, sb);
                else if (o is Guid)
                {
                    SerializeGuid((Guid)o, sb);
                }
                else
                {
                    var uri = o as Uri;

                    if (uri != (Uri)null)
                        SerializeUri(uri, sb);
                    else if (o is double)
                        sb.Append(((double)o).ToString("r", (IFormatProvider)CultureInfo.InvariantCulture));
                    else if (o is float)
                        sb.Append(((float)o).ToString("r", (IFormatProvider)CultureInfo.InvariantCulture));
                    else if (o.GetType().IsPrimitive || o is Decimal)
                    {
                        IConvertible convertible = o as IConvertible;
                        if (convertible != null)
                            sb.Append(convertible.ToString((IFormatProvider)CultureInfo.InvariantCulture));
                        else
                            sb.Append(o.ToString());
                    }
                    else
                    {
                        var type = o.GetType();

                        if (type.IsEnum)
                        {
                            var underlyingType = Enum.GetUnderlyingType(type);

                            if (underlyingType == typeof(long) || underlyingType == typeof(ulong))
                                throw new InvalidOperationException();

                            sb.Append(((Enum)o).ToString("D"));
                        }
                        else
                        {
                            try
                            {
                                if (objectsInUse == null)
                                    objectsInUse = new Hashtable((IEqualityComparer)new ReferenceComparer());
                                else if (objectsInUse.ContainsKey(o))
                                    throw new InvalidOperationException("The object graph is recursive.");

                                objectsInUse.Add(o, (object)null);

                                var o1 = o as IDictionary;

                                if (o1 != null)
                                {
                                    this.SerializeDictionary(o1, sb, depth, objectsInUse);
                                }
                                else
                                {
                                    var enumerable = o as IEnumerable;

                                    if (enumerable != null)
                                        this.SerializeEnumerable(enumerable, sb, depth, objectsInUse);
                                    else
                                        this.SerializeCustomObject(o, sb, depth, objectsInUse);
                                }
                            }
                            finally
                            {
                                if (objectsInUse != null)
                                    objectsInUse.Remove(o);
                            }
                        }
                    }
                }
            }
        }

        #region Security Stuff
        static object FieldInfoGetValue(FieldInfo field, object target)
        {
            Type declaringType = field.DeclaringType;
            if (declaringType == (Type)null)
            {
                if (!field.IsPublic)
                    DemandGrantSet(field.Module.Assembly);
            }
            else if (!(declaringType != (Type)null) || !declaringType.IsVisible || !field.IsPublic)
                DemandReflectionAccess(declaringType);
            return field.GetValue(target);
        }

        static void DemandReflectionAccess(Type type)
        {
            try
            {
                MemberAccessPermission.Demand();
            }
            catch (SecurityException)
            {
                DemandGrantSet(type.Assembly);
            }
        }

        static ReflectionPermission memberAccessPermission;
        static ReflectionPermission restrictedMemberAccessPermission;

        static ReflectionPermission MemberAccessPermission
        {
            get
            {
                if (memberAccessPermission == null)
                    memberAccessPermission = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
                return memberAccessPermission;
            }
        }

        static ReflectionPermission RestrictedMemberAccessPermission
        {
            get
            {
                if (restrictedMemberAccessPermission == null)
                    restrictedMemberAccessPermission = new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess);
                return restrictedMemberAccessPermission;
            }
        }

        [SecuritySafeCritical]
        static void DemandGrantSet(Assembly assembly)
        {
            PermissionSet permissionSet = assembly.PermissionSet;
            permissionSet.AddPermission((IPermission)RestrictedMemberAccessPermission);
            permissionSet.Demand();
        }

        static object MethodInfoInvoke(MethodInfo method, object target, object[] args)
        {
            Type declaringType = method.DeclaringType;
            if (declaringType == (Type)null)
            {
                if (!method.IsPublic || !GenericArgumentsAreVisible(method))
                    DemandGrantSet(method.Module.Assembly);
            }
            else if (!declaringType.IsVisible || !method.IsPublic || !GenericArgumentsAreVisible(method))
                DemandReflectionAccess(declaringType);
            return method.Invoke(target, args);
        }

        private static bool GenericArgumentsAreVisible(MethodInfo method)
        {
            if (method.IsGenericMethod)
            {
                foreach (Type type in method.GetGenericArguments())
                {
                    if (!type.IsVisible)
                        return false;
                }
            }
            return true;
        }
        #endregion

        private class ReferenceComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
            {
                return x == y;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
