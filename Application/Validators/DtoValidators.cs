using FluentValidation;
using back_tienda.Core.DTOs;

namespace back_tienda.Application.Validators;

public class LoginValidator : AbstractValidator<LoginRequestDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Usuario)
            .NotEmpty().WithMessage("El usuario es requerido")
            .MinimumLength(3).WithMessage("El usuario debe tener al menos 3 caracteres");

        RuleFor(x => x.Contraseña)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
    }
}

public class RegistroUsuarioValidator : AbstractValidator<RegistroUsuarioDto>
{
    public RegistroUsuarioValidator()
    {
        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Usuario)
            .NotEmpty().WithMessage("El usuario es requerido")
            .MinimumLength(3).WithMessage("El usuario debe tener al menos 3 caracteres")
            .MaximumLength(100).WithMessage("El usuario no puede exceder 100 caracteres")
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("El usuario solo puede contener letras, números, puntos, guiones y guiones bajos");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es requerido")
            .EmailAddress().WithMessage("El correo no es válido")
            .MaximumLength(150).WithMessage("El correo no puede exceder 150 caracteres");

        RuleFor(x => x.Contraseña)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una minúscula")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número")
            .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un carácter especial");

        RuleFor(x => x.Rol)
            .IsInEnum().WithMessage("El rol no es válido");
    }
}

public class CrearTiendaValidator : AbstractValidator<CrearTiendaDto>
{
    public CrearTiendaValidator()
    {
        RuleFor(x => x.NombreTienda)
            .NotEmpty().WithMessage("El nombre de la tienda es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Telefono)
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Telefono));

        RuleFor(x => x.CorreoTienda)
            .EmailAddress().WithMessage("El correo no es válido")
            .MaximumLength(150).WithMessage("El correo no puede exceder 150 caracteres")
            .When(x => !string.IsNullOrEmpty(x.CorreoTienda));

        RuleFor(x => x.Nit)
            .MaximumLength(50).WithMessage("El NIT no puede exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Nit));

        RuleFor(x => x.IdDueño)
            .NotEmpty().WithMessage("El dueño es requerido");
    }
}

public class CrearProductoValidator : AbstractValidator<CrearProductoDto>
{
    public CrearProductoValidator()
    {
        RuleFor(x => x.NombreProducto)
            .NotEmpty().WithMessage("El nombre del producto es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.CodigoProducto)
            .MaximumLength(100).WithMessage("El código no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.CodigoProducto));

        RuleFor(x => x.PrecioVenta)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a 0");

        RuleFor(x => x.PrecioCompra)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de compra debe ser mayor o igual a 0")
            .When(x => x.PrecioCompra.HasValue);

        RuleFor(x => x.StockActual)
            .GreaterThanOrEqualTo(0).WithMessage("El stock actual no puede ser negativo");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo");

        RuleFor(x => x.UnidadMedida)
            .NotEmpty().WithMessage("La unidad de medida es requerida")
            .MaximumLength(50).WithMessage("La unidad de medida no puede exceder 50 caracteres");

        // IdTienda es opcional en el DTO porque puede venir del token
        // RuleFor(x => x.IdTienda)
        //     .NotEmpty().WithMessage("La tienda es requerida");
    }
}

public class CrearVentaValidator : AbstractValidator<CrearVentaDto>
{
    public CrearVentaValidator()
    {
        RuleFor(x => x.IdTienda)
            .NotEmpty().WithMessage("La tienda es requerida");

        RuleFor(x => x.MetodoPago)
            .MaximumLength(50).WithMessage("El método de pago no puede exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.MetodoPago));

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("Debe agregar al menos un producto a la venta")
            .Must(detalles => detalles.Count > 0).WithMessage("Debe agregar al menos un producto");

        RuleForEach(x => x.Detalles).SetValidator(new CrearDetalleVentaValidator());
    }
}

public class CrearDetalleVentaValidator : AbstractValidator<CrearDetalleVentaDto>
{
    public CrearDetalleVentaValidator()
    {
        RuleFor(x => x.IdProducto)
            .NotEmpty().WithMessage("El producto es requerido");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");

        RuleFor(x => x.PrecioUnitario)
            .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a 0");
    }
}

public class CrearCompraValidator : AbstractValidator<CrearCompraDto>
{
    public CrearCompraValidator()
    {
// RuleFor(x => x.IdTienda)
//     .NotEmpty().WithMessage("La tienda es requerida");

        RuleFor(x => x.NumeroFactura)
            .MaximumLength(100).WithMessage("El número de factura no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.NumeroFactura));

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("Debe agregar al menos un producto a la compra")
            .Must(detalles => detalles.Count > 0).WithMessage("Debe agregar al menos un producto");

        RuleForEach(x => x.Detalles).SetValidator(new CrearDetalleCompraValidator());
    }
}

public class CrearDetalleCompraValidator : AbstractValidator<CrearDetalleCompraDto>
{
    public CrearDetalleCompraValidator()
    {
        RuleFor(x => x.IdProducto)
            .NotEmpty().WithMessage("El producto es requerido");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");

        RuleFor(x => x.PrecioUnitario)
            .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a 0");
    }
}
