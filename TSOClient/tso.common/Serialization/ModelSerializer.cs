﻿using FSO.Common.Serialization.TypeSerializers;
using Mina.Core.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSO.Common.Serialization
{
    public class ModelSerializer : IModelSerializer
    {
        private List<ITypeSerializer> TypeSerializers = new List<ITypeSerializer>();

        public ModelSerializer(){
            //Built-in
            AddTypeSerializer(new cTSOValueBoolean());
            AddTypeSerializer(new cTSOValueString());
            AddTypeSerializer(new cTSOValueStringVector());
            AddTypeSerializer(new cTSOValueByte());
            AddTypeSerializer(new cTSOValueByteVector());
            AddTypeSerializer(new cTSOValueSByte());
            AddTypeSerializer(new cTSOValueSByteVector());

            AddTypeSerializer(new cTSOValueUInt32());
            AddTypeSerializer(new cTSOValueUInt16());
            AddTypeSerializer(new cTSOValueDecorated());
        }


        public uint? GetClsid(object value)
        {
            if (value == null) { return null; }
            var serializer = GetSerializer(value.GetType());
            if (serializer == null) { return null; }
            return serializer.GetClsid(value);
        }

        public void Serialize(IoBuffer output, object obj, ISerializationContext context)
        {
            if (obj == null) { return; }
            var serializer = GetSerializer(obj.GetType());
            if (serializer == null) { return; }

            serializer.Serialize(output, obj, context);
        }


        public void Serialize(IoBuffer output, object value, ISerializationContext context, bool clsIdPrefix)
        {
            if (value == null) { return; }
            var serializer = GetSerializer(value.GetType());
            if (serializer == null) { return; }

            if (clsIdPrefix){
                output.PutUInt32(serializer.GetClsid(value).Value);
            }
            serializer.Serialize(output, value, context);
        }

        public IoBuffer SerializeBuffer(object value, ISerializationContext context, bool clsIdPrefix)
        {
            var buffer = IoBuffer.Allocate(256);
            buffer.AutoExpand = true;
            buffer.Order = ByteOrder.BigEndian;
            Serialize(buffer, value, context, clsIdPrefix);
            buffer.Flip();
            return buffer;
        }

        public object Deserialize(uint clsid, IoBuffer input, ISerializationContext context)
        {
            var serializer = GetSerializer(clsid);
            if (serializer == null) { return null; }

            return serializer.Deserialize(clsid, input, context);
        }

        public void AddTypeSerializer(ITypeSerializer serializer)
        {
            TypeSerializers.Add(serializer);
        }

        private ITypeSerializer GetSerializer(uint clsid){
            return TypeSerializers.FirstOrDefault(x => x.CanDeserialize(clsid));
        }

        private ITypeSerializer GetSerializer(Type type){
            return TypeSerializers.FirstOrDefault(x => x.CanSerialize(type));
        }
    }
}
