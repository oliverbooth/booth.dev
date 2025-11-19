using DEDrake;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoothDotDev.Data.Web.ValueConverters;

internal sealed class ShortGuidToGuidConverter() : ValueConverter<ShortGuid, Guid>(v => v, v => v);
