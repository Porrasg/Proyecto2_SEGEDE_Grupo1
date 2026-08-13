namespace CoreApp
{
    /// <summary>
    /// Error de validación o regla de negocio que puede devolverse al cliente sin
    /// revelar detalles internos de infraestructura.
    /// </summary>
    public sealed class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }
    }
}
