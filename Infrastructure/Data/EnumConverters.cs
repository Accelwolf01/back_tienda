using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using back_tienda.Core.Enums;

namespace back_tienda.Infrastructure.Data;

public class EstadoUsuarioConverter : ValueConverter<EstadoUsuario, string>
{
    public EstadoUsuarioConverter() : base(
        v => v.ToString().ToUpper(),
        v => (EstadoUsuario)Enum.Parse(typeof(EstadoUsuario), v, true))
    {
    }
}

public class TipoRolConverter : ValueConverter<TipoRol, string>
{
    public TipoRolConverter() : base(
        v => v.ToString().ToUpper(),
        v => (TipoRol)Enum.Parse(typeof(TipoRol), v, true))
    {
    }
}

public class EstadoDocumentoConverter : ValueConverter<EstadoDocumento, string>
{
    public EstadoDocumentoConverter() : base(
        v => v.ToString().ToUpper(),
        v => (EstadoDocumento)Enum.Parse(typeof(EstadoDocumento), v, true))
    {
    }
}
