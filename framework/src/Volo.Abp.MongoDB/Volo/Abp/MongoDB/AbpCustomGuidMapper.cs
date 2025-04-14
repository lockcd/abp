using System;
using MongoDB.Bson;

namespace Volo.Abp.MongoDB;

public class AbpCustomGuidMapper : ICustomBsonTypeMapper
{
    public bool TryMapToBsonValue(object value, out BsonValue bsonValue)
    {
        bsonValue = new BsonBinaryData((Guid)value, GuidRepresentation.Standard);
        return true;
    }
}
