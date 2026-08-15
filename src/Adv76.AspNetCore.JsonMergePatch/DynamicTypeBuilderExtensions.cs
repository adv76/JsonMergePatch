using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Adv76.JsonMergePatch;

namespace Adv76.AspNetCore.JsonMergePatch;

internal static class DynamicTypeBuilderExtensions
{
    private const string Name = "Adv76.AspNetCore.JsonMergePatch.__DynamicPatchDocumentAssembly";

    private static ModuleBuilder? _moduleBuilder = null;

    private static ModuleBuilder GetModuleBuilder()
    {
        if (_moduleBuilder is null)
        {
            var assemblyName = new AssemblyName(Name);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                assemblyName,
                AssemblyBuilderAccess.Run);

            _moduleBuilder = assemblyBuilder.DefineDynamicModule(Name);
        }

        return _moduleBuilder;
    }

    internal static Type BuildPatchType(this JsonTypeInfo t, JsonMergeOptions mergeOptions,
        JsonSerializerOptions jsonSerializerOptions)
    {
        var mb = GetModuleBuilder();

        var existingType = mb.GetType(t.Type.Name + "MergePatch");
        if (existingType is not null)
        {
            Debug.WriteLine("Type was cached");
            return existingType;
        }
        
        Debug.WriteLine("Building type...");
        
        TypeBuilder tb = mb.DefineType(
            t.Type.Name + "MergePatch",
            TypeAttributes.NotPublic | TypeAttributes.Class);

        foreach (var property in t.Properties)
        {
            tb.BuildProperty(property, mergeOptions, jsonSerializerOptions);
        }

        return tb.CreateType();
    }

    private static void BuildProperty(this TypeBuilder tb, JsonPropertyInfo property, JsonMergeOptions mergeOptions,
        JsonSerializerOptions jsonSerializerOptions)
    {
        if (!IsPropertyPatchable(property, mergeOptions))
        {
            return;
        }
        
        var propertyType =  property.PropertyType;
        
        var typeInfo = jsonSerializerOptions.GetTypeInfo(property.PropertyType);
        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            propertyType = typeInfo.BuildPatchType(mergeOptions, jsonSerializerOptions);
        }

        if (property.PropertyType.IsValueType && !(property.PropertyType.IsGenericType &&
                                                   property.PropertyType.GetGenericTypeDefinition() ==
                                                   typeof(Nullable<>)))
        {
            propertyType = typeof(Nullable<>).MakeGenericType(propertyType);
        }
        
        FieldBuilder fb = tb.DefineField(
            "f_" + property.Name,
            propertyType,
            FieldAttributes.Private);

        PropertyBuilder pb = tb.DefineProperty(property.Name, PropertyAttributes.None, propertyType, null);

        // The property "set" and property "get" methods require a special
        // set of attributes.
        MethodAttributes getSetAttr = MethodAttributes.Public |
                                      MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        // Define the "get" accessor method for Number. The method returns
        // an integer and has no arguments. (Note that null could be
        // used instead of Types.EmptyTypes)
        MethodBuilder mbNumberGetAccessor = tb.DefineMethod(
            "get_Number",
            getSetAttr,
            typeof(int),
            Type.EmptyTypes);

        ILGenerator numberGetIL = mbNumberGetAccessor.GetILGenerator();
        // For an instance property, argument zero is the instance. Load the
        // instance, then load the private field and return, leaving the
        // field value on the stack.
        numberGetIL.Emit(OpCodes.Ldarg_0);
        numberGetIL.Emit(OpCodes.Ldfld, fb);
        numberGetIL.Emit(OpCodes.Ret);

        // Define the "set" accessor method for Number, which has no return
        // type and takes one argument of type int (Int32).
        MethodBuilder mbNumberSetAccessor = tb.DefineMethod(
            "set_Number",
            getSetAttr,
            null,
            new Type[] { propertyType });

        ILGenerator numberSetIL = mbNumberSetAccessor.GetILGenerator();
        // Load the instance and then the numeric argument, then store the
        // argument in the field.
        numberSetIL.Emit(OpCodes.Ldarg_0);
        numberSetIL.Emit(OpCodes.Ldarg_1);
        numberSetIL.Emit(OpCodes.Stfld, fb);
        numberSetIL.Emit(OpCodes.Ret);

        // Last, map the "get" and "set" accessor methods to the
        // PropertyBuilder. The property is now complete.
        pb.SetGetMethod(mbNumberGetAccessor);
        pb.SetSetMethod(mbNumberSetAccessor);
    }
    
    private static bool IsPropertyPatchable(JsonPropertyInfo propertyInfo, JsonMergeOptions mergeOptions)
    {
        if (propertyInfo.AttributeProvider is null)
        {
            return false;
        }
        
        var attributes = propertyInfo.AttributeProvider.GetCustomAttributes(typeof(JsonMergePropertySecurityAttribute), true);
        if (attributes.Length > 0 && attributes[^1] is JsonMergePropertySecurityAttribute attribute)
        {
            return attribute.Policy == JsonMergeSecurityPolicy.AllowPatching;
        }

        return mergeOptions.SecurityPolicy == JsonMergeSecurityPolicy.AllowPatching;
    }
}