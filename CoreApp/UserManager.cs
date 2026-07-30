using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace CoreApp
{
    // Lógica de negocio de los usuarios
    public class UserManager
    {
        // Datos estaticos para probar funcionalidades dentro de la app
        private static readonly Dictionary<string, (string Password, string Role, string FirstName, string LastName, int Id)> StaticLoginAccounts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["admin@segede.local"] = ("Admin123!", "Admin", "Administrador", "Sistema", -1001),
            ["engineer@segede.local"] = ("Eng123!", "Engineer", "Ingeniero", "Sistema", -1002),
            ["buyer@segede.local"] = ("Buyer123!", "Buyer", "Comprador", "Sistema", -1003)
        };

        public List<User> RetrieveAllUsers()
        {
            var uCrud = new UserCrudFactory();
            return uCrud.RetrieveAll<User>();
        }

        public void Create(User user)
        {
            if (user == null)
            {
                throw new Exception("El usuario no puede ser nulo");
            }

            ValidateUser(user); // Validación de campos obligatorios y formatos

            var uCrud = new UserCrudFactory();

            // Verifica si ya existe un usuario con el mismo correo electrónico
            var userByEmail = uCrud.RetrieveByEmail(user.Email);

            if (userByEmail != null)
            {
                throw new Exception("Ya existe un usuario registrado con ese correo electrónico");
            }

            // Verifica si ya existe un usuario con la misma identificación
            var userByIdentification = uCrud.RetrieveByIdentification(user.Identification);

            if (userByIdentification != null)
            {
                throw new Exception("Ya existe un usuario registrado con esa identificación");
            }

            // La edad se calcula a partir de la fecha de nacimiento
            user.Age = CalculateAge(user.BirthDate);

            // La contraseña se guarda cifrada para cumplir con la rúbrica de seguridad
            user.Password = HashPassword(user.Password);

            // Valores iniciales del usuario
            user.Status = "Pending";
            user.FailedLoginAttempts = 0;
            user.LockoutEndAt = null;
            user.LastLoginAt = null;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = null;

            uCrud.Create(user);

            // Después de crear el usuario, se genera y envía un OTP para la activación de la cuenta
            var otpManager = new OtpManager();

            otpManager.GenerateAndSendOtp(user.Email, user.FirstName, "ACCOUNT_ACTIVATION");
        }

        public void Update(User user)
        {
            if (user == null)
            {
                throw new Exception("El usuario no puede ser nulo");
            }

            if (user.Id <= 0)
            {
                throw new Exception("El identificador del usuario no es válido");
            }

            var uCrud = new UserCrudFactory();

            var currentUser = uCrud.RetrieveById<User>(user.Id);

            if (currentUser == null)
            {
                throw new Exception("El usuario que desea actualizar no existe");
            }

            // Verifica si ya existe otro usuario con el mismo correo electrónico
            var userByEmail = uCrud.RetrieveByEmail(user.Email);

            if (userByEmail != null && userByEmail.Id != user.Id)
            {
                throw new Exception("Ya existe otro usuario registrado con ese correo electrónico");
            }

            // Verifica si ya existe otro usuario con la misma identificación
            var userByIdentification = uCrud.RetrieveByIdentification(user.Identification);

            if (userByIdentification != null && userByIdentification.Id != user.Id)
            {
                throw new Exception("Ya existe otro usuario registrado con esa identificación");
            }

            // Guarda el valor original para evitar rehash accidental cuando se conserva la contraseña actual.
            var originalPassword = user.Password;

            // Si no se envía una nueva contraseña, se conserva la actual para evitar fallos de validación.
            user.Password = string.IsNullOrWhiteSpace(originalPassword)
                ? currentUser.Password
                : originalPassword;

            ValidateUserForUpdate(user, currentUser);

            // Se vuelve a calcular la edad
            user.Age = CalculateAge(user.BirthDate);

            // Si la contraseña viene vacía, se conserva la actual; si viene informada, se re-hashea una sola vez.
            user.Password = string.IsNullOrWhiteSpace(originalPassword)
                ? currentUser.Password
                : (IsHashedPassword(originalPassword) ? originalPassword : HashPassword(originalPassword));

            // Se conserva la fecha original de creación
            user.CreatedAt = currentUser.CreatedAt;
            // Se actualiza la fecha de modificación
            user.UpdatedAt = DateTime.Now;

            uCrud.Update(user);
        }

        public void Delete(User user)
        {
            if (user == null)
            {
                throw new Exception("El usuario no puede ser nulo");
            }

            if (user.Id <= 0)
            {
                throw new Exception("El identificador del usuario no es válido");
            }

            var uCrud = new UserCrudFactory();

            var currentUser = uCrud.RetrieveById<User>(user.Id);

            if (currentUser == null)
            {
                throw new Exception("El usuario que desea eliminar no existe");
            }

            if (currentUser.Status == "Inactive")
            {
                throw new Exception("El usuario ya se encuentra inactivo");
            }

            // La eliminación es lógica
            user.Status = "Inactive";
            // Se actualiza la fecha de modificación (cuando fue inactivado)
            user.UpdatedAt = DateTime.Now;

            uCrud.Delete(user);
        }

        public User RetrieveUserById(int id)
        {
            if (id <= 0)
            {
                throw new Exception("El identificador del usuario no es válido");
            }

            var uCrud = new UserCrudFactory();

            var user = uCrud.RetrieveById<User>(id);

            if (user == null)
            {
                throw new Exception("No se encontró el usuario solicitado");
            }

            return user;
        }

        public User RetrieveUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El correo electrónico es requerido");
            }

            if (!IsValidEmail(email))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            var uCrud = new UserCrudFactory();

            var user = uCrud.RetrieveByEmail(email);

            if (user == null)
            {
                throw new Exception("No se encontró un usuario con ese correo electrónico");
            }

            return user;
        }

        public User RetrieveUserByIdentification(string identification)
        {
            if (string.IsNullOrWhiteSpace(identification))
            {
                throw new Exception("La identificación es requerida");
            }

            var uCrud = new UserCrudFactory();

            var user = uCrud.RetrieveByIdentification(identification);

            if (user == null)
            {
                throw new Exception("No se encontró un usuario con esa identificación");
            }

            return user;
        }

        public User Login(string email, string password)
        {
            // dato estaticos
            email = email?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El correo electrónico es requerido");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("La contraseña es requerida");
            }

            if (!IsValidEmail(email))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            // datos estaticos
            if (TryLoginStaticAccount(email, password, out User staticUser))
            {
                return staticUser;
            }

            var uCrud = new UserCrudFactory();

            var user = uCrud.RetrieveByEmail(email);

            if (user == null)
            {
                throw new Exception("El correo electrónico o la contraseña son incorrectos");
            }

            // Verifica el estado de la cuenta
            if (user.Status == "Pending")
            {
                throw new Exception("La cuenta todavía no ha sido activada");
            }

            if (user.Status == "Locked")
            {
                throw new Exception("La cuenta se encuentra bloqueada");
            }

            // Verifica si el bloqueo temporal continúa activo
            if (user.LockoutEndAt != null && user.LockoutEndAt > DateTime.Now)
            {
                throw new Exception("La cuenta se encuentra bloqueada temporalmente");
            }

            // Si el tiempo de bloqueo terminó, reinicia los intentos
            if (user.LockoutEndAt != null && user.LockoutEndAt <= DateTime.Now)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEndAt = null;
                user.UpdatedAt = DateTime.Now;

                uCrud.UpdateLoginAttempts(user);
            }

            // Verifica la contraseña
            if (!VerifyPassword(user.Password, password))
            {
                user.FailedLoginAttempts++;

                user.UpdatedAt = DateTime.Now;

                // Después de tres intentos, bloquea temporalmente
                if (user.FailedLoginAttempts >= 3)
                {
                    user.LockoutEndAt = DateTime.Now.AddMinutes(15);
                    uCrud.UpdateLoginAttempts(user);
                    throw new Exception("3 intentos agotados. Reintente en 15 minutos");
                }

                uCrud.UpdateLoginAttempts(user);

                throw new Exception("El correo electrónico o la contraseña son incorrectos");
            }

            // Las credenciales son correctas, se reinician los intentos fallidos
            user.FailedLoginAttempts = 0;
            user.LockoutEndAt = null;
            user.UpdatedAt = DateTime.Now;

            uCrud.UpdateLoginAttempts(user);

            // Se genera y envía el OTP para completar el inicio de sesión
            var otpManager = new OtpManager();

            otpManager.GenerateAndSendOtp(user.Email, user.FirstName, "LOGIN");

            return user;
        }

        // Igualmente para datos estaticos para probar funcionalidad dentro de app 
        private bool TryLoginStaticAccount(string email, string password, out User user)
        {
            user = null;

            if (!StaticLoginAccounts.TryGetValue(email, out var account))
            {
                return false;
            }

            if (!string.Equals(account.Password, password, StringComparison.Ordinal))
            {
                return false;
            }

            var uCrud = new UserCrudFactory();
            var storedUser = uCrud.RetrieveByEmail(email);

            if (storedUser != null)
            {
                storedUser.Role = string.IsNullOrWhiteSpace(storedUser.Role) ? account.Role : storedUser.Role;
                storedUser.Status = "Active";
                user = storedUser;
                return true;
            }

            user = new User
            {
                Id = account.Id,
                Email = email,
                FirstName = account.FirstName,
                FirstLastName = account.LastName,
                Role = account.Role,
                Status = "Active",
                FailedLoginAttempts = 0,
                LockoutEndAt = null,
                LastLoginAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return true;
        }

        // Completa el inicio de sesión después de validar el OTP
        public User ValidateLoginOtp(string email, string tokenCode)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El correo electrónico es requerido");
            }

            if (!IsValidEmail(email))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            if (string.IsNullOrWhiteSpace(tokenCode))
            {
                throw new Exception("El código OTP es requerido");
            }

            var uCrud = new UserCrudFactory();

            var user = uCrud.RetrieveByEmail(email);

            if (user == null)
            {
                throw new Exception("No existe un usuario registrado con ese correo electrónico");
            }

            if (user.Status != "Active")
            {
                throw new Exception("La cuenta no se encuentra disponible para iniciar sesión");
            }

            // Se valida el código OTP
            var otpManager = new OtpManager();

            otpManager.ValidateOtp(email, tokenCode, "LOGIN");

            // Las credenciales son correctas, se reinician los intentos fallidos
            user.FailedLoginAttempts = 0;
            user.LockoutEndAt = null;
            user.LastLoginAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;

            uCrud.UpdateLastLogin(user);

            // Auditar el inicio de sesión exitoso; un fallo de auditoría nunca debe
            // impedir un login ya validado.
            try
            {
                new AuditManager().Create(new Audit
                {
                    UserId = user.Id,
                    Action = "Login",
                    EntityName = "Users",
                    EntityId = user.Id,
                    Description = $"Inicio de sesión exitoso: {user.Email}"
                });
            }
            catch
            {
                // No bloquear el login por un fallo de auditoría
            }

            return user;
        }

        // Cambio de contraseña sin sesión: valida correo + contraseña actual y envía OTP
        public void ChangePassword(string email, string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El correo electrónico es requerido");
            }

            if (!IsValidEmail(email))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                throw new Exception("La contraseña actual es requerida");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new Exception("La nueva contraseña es requerida");
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                throw new Exception("La confirmación de la contraseña es requerida");
            }

            if (newPassword != confirmPassword)
            {
                throw new Exception("La nueva contraseña y la confirmación no coinciden");
            }

            if (!IsValidPassword(newPassword))
            {
                throw new Exception("La nueva contraseña debe tener al menos 8 caracteres, " +
                    "una mayúscula, una minúscula, un número y un carácter especial");
            }

            var uCrud = new UserCrudFactory();
            var user = uCrud.RetrieveByEmail(email);

            if (user == null)
            {
                throw new Exception("No existe un usuario registrado con ese correo electrónico");
            }

            if (user.Status != "Active")
            {
                throw new Exception("La cuenta no se encuentra activa");
            }

            if (!VerifyPassword(user.Password, currentPassword))
            {
                throw new Exception("La contraseña actual es incorrecta");
            }

            if (currentPassword == newPassword)
            {
                throw new Exception("La nueva contraseña debe ser diferente a la contraseña actual");
            }

            var otpManager = new OtpManager();
            otpManager.GenerateAndSendOtp(user.Email, user.FirstName, "CHANGE_PASSWORD");
        }

        // Cambio de contraseña CON sesión activa: el usuario ya está autenticado,
        // así que se verifica identidad con la contraseña actual (no con OTP) y se
        // aplica el cambio de inmediato. Distinto de ChangePassword/ConfirmChangePassword
        // (email + OTP), que es exclusivamente para "olvidé mi contraseña" sin sesión.
        public void ChangePasswordAuthenticated(int userId, string currentPassword, string newPassword, string confirmPassword)
        {
            if (userId <= 0)
            {
                throw new Exception("Sesión inválida. Inicie sesión nuevamente");
            }

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                throw new Exception("La contraseña actual es requerida");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new Exception("La nueva contraseña es requerida");
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                throw new Exception("La confirmación de la contraseña es requerida");
            }

            if (newPassword != confirmPassword)
            {
                throw new Exception("La nueva contraseña y la confirmación no coinciden");
            }

            if (!IsValidPassword(newPassword))
            {
                throw new Exception("La nueva contraseña debe tener al menos 8 caracteres, " +
                    "una mayúscula, una minúscula, un número y un carácter especial");
            }

            var uCrud = new UserCrudFactory();
            var user = uCrud.RetrieveById<User>(userId);

            if (user == null)
            {
                throw new Exception("No se encontró el usuario de la sesión actual");
            }

            if (user.Status != "Active")
            {
                throw new Exception("La cuenta no se encuentra activa");
            }

            if (!VerifyPassword(user.Password, currentPassword))
            {
                throw new Exception("La contraseña actual es incorrecta");
            }

            if (currentPassword == newPassword)
            {
                throw new Exception("La nueva contraseña debe ser diferente a la contraseña actual");
            }

            user.Password = HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;
            uCrud.UpdatePassword(user);
        }

        // Cambio de contraseña sin sesión: valida correo + OTP y actualiza la clave
        public void ConfirmChangePassword(string email, string tokenCode, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El correo electrónico es requerido");
            }

            if (!IsValidEmail(email))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            if (string.IsNullOrWhiteSpace(tokenCode))
            {
                throw new Exception("El código OTP es requerido");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new Exception("La nueva contraseña es requerida");
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                throw new Exception("La confirmación de la contraseña es requerida");
            }

            if (newPassword != confirmPassword)
            {
                throw new Exception("La nueva contraseña y la confirmación no coinciden");
            }

            if (!IsValidPassword(newPassword))
            {
                throw new Exception("La nueva contraseña debe tener al menos 8 caracteres, " +
                    "una mayúscula, una minúscula, un número y un carácter especial");
            }

            var uCrud = new UserCrudFactory();
            var user = uCrud.RetrieveByEmail(email);

            if (user == null)
            {
                throw new Exception("No existe un usuario registrado con ese correo electrónico");
            }

            if (user.Status != "Active")
            {
                throw new Exception("La cuenta no se encuentra activa");
            }

            if (user.Password == newPassword)
            {
                throw new Exception("La nueva contraseña debe ser diferente a la contraseña actual");
            }

            var otpManager = new OtpManager();
            otpManager.ValidateOtp(user.Email, tokenCode, "CHANGE_PASSWORD");

            user.Password = HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;
            uCrud.UpdatePassword(user);
        }

        // Solicita el restablecimiento de contraseña y envía un OTP
        public void ResetPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El correo electrónico es requerido");
            }

            if (!IsValidEmail(email))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            var uCrud = new UserCrudFactory();
            var otpManager = new OtpManager();

            var user = uCrud.RetrieveByEmail(email);

            if (user == null)
            {
                throw new Exception("No existe un usuario registrado con ese correo electrónico");
            }

            if (user.Status == "Pending")
            {
                throw new Exception("La cuenta todavía no ha sido activada");
            }

            if (user.Status == "Inactive")
            {
                throw new Exception("La cuenta se encuentra inactiva");
            }

            if (user.Status == "Locked")
            {
                throw new Exception("La cuenta se encuentra bloqueada");
            }

            otpManager.GenerateAndSendOtp(user.Email, user.FirstName, "RESET_PASSWORD");
        }

        // Confirma el restablecimiento después de validar el OTP
        public void ConfirmResetPassword(string email, string tokenCode, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El correo electrónico es requerido");
            }

            if (!IsValidEmail(email))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            if (string.IsNullOrWhiteSpace(tokenCode))
            {
                throw new Exception("El código OTP es requerido");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new Exception("La nueva contraseña es requerida");
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                throw new Exception("La confirmación de la contraseña es requerida");
            }

            if (newPassword != confirmPassword)
            {
                throw new Exception("La nueva contraseña y la confirmación no coinciden");
            }

            if (!IsValidPassword(newPassword))
            {
                throw new Exception("La nueva contraseña debe tener al menos 8 caracteres, " +
                    "una mayúscula, una minúscula, un número y un carácter especial");
            }

            var uCrud = new UserCrudFactory();

            var user = uCrud.RetrieveByEmail(email);

            if (user == null)
            {
                throw new Exception("No existe un usuario registrado con ese correo electrónico");
            }

            if (user.Status != "Active")
            {
                throw new Exception("La cuenta no se encuentra activa");
            }

            if (user.Password == newPassword)
            {
                throw new Exception("La nueva contraseña debe ser diferente a la contraseña anterior");
            }

            newPassword = HashPassword(newPassword);

            // Se valida el código OTP
            var otpManager = new OtpManager();

            otpManager.ValidateOtp(user.Email, tokenCode, "RESET_PASSWORD");

            // La contraseña actual es correcta y el OTP es válido, se actualiza la contraseña
            user.Password = newPassword;
            user.FailedLoginAttempts = 0;
            user.LockoutEndAt = null;
            user.UpdatedAt = DateTime.Now;

            uCrud.UpdatePassword(user);
        }

        // Activa la cuenta después de validar el código OTP y establece la contraseña
        // definitiva elegida por el usuario. Es el único momento en que un usuario
        // creado por un administrador (que nunca recibe una contraseña real utilizable
        // de manos del admin) define su propia contraseña; para el auto-registro de
        // compradores esto simplemente confirma/reemplaza la que ya eligió al registrarse.
        public void ActivateAccount(string email, string tokenCode, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("El correo electrónico es requerido");
            }

            if (!IsValidEmail(email))
            {
                throw new Exception( "El correo electrónico no tiene un formato válido");
            }

            if (string.IsNullOrWhiteSpace(tokenCode))
            {
                throw new Exception("El código OTP es requerido");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                throw new Exception("Debe establecer y confirmar su nueva contraseña");
            }

            if (newPassword != confirmPassword)
            {
                throw new Exception("La nueva contraseña y la confirmación no coinciden");
            }

            if (!IsValidPassword(newPassword))
            {
                throw new Exception("La contraseña debe tener al menos 8 caracteres, " +
                    "una mayúscula, una minúscula, un número y un carácter especial");
            }

            var uCrud = new UserCrudFactory();
            var otpManager = new OtpManager();

            var user = uCrud.RetrieveByEmail(email);

            if (user == null)
            {
                throw new Exception("No existe un usuario registrado con ese correo electrónico");
            }

            if (user.Status == "Active")
            {
                throw new Exception("La cuenta ya se encuentra activa");
            }

            if (user.Status == "Locked")
            {
                throw new Exception("La cuenta se encuentra bloqueada");
            }

            if (user.Status != "Pending")
            {
                throw new Exception("La cuenta no se encuentra pendiente de activación");
            }

            // Primero se valida que el OTP sea correcto y vigente en la capa de negocio.
            otpManager.ValidateOtp(email, tokenCode, "ACCOUNT_ACTIVATION");

            // Luego se ejecuta la activación en base de datos y se fija la contraseña
            // definitiva elegida por el usuario.
            uCrud.ActivateAccount(email, tokenCode);

            user.Password = HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;
            uCrud.UpdatePassword(user);
        }

        // Encripta la contraseña utilizando PBKDF2 con SHA256 y un salt aleatorio
        private string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return password;
            }

            const int iterations = 100_000;
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private bool VerifyPassword(string storedPassword, string enteredPassword)
        {
            if (string.IsNullOrWhiteSpace(storedPassword) || string.IsNullOrWhiteSpace(enteredPassword))
            {
                return false;
            }

            if (storedPassword.Contains('.'))
            {
                var parts = storedPassword.Split('.');
                if (parts.Length != 3) return false;

                if (!int.TryParse(parts[0], out int iterations)) return false;

                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] expectedHash = Convert.FromBase64String(parts[2]);

                using var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, iterations, HashAlgorithmName.SHA256);
                byte[] actualHash = pbkdf2.GetBytes(expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }

            return storedPassword == enteredPassword;
        }

        // VALIDACIONES 

        private void ValidateUser(User user)
        {
            if (HasEmptyFields(user))
            {
                throw new Exception("Todos los campos obligatorios son requeridos");
            }

            if (!IsValidBirthDate(user.BirthDate))
            {
                throw new Exception("La fecha de nacimiento no es válida");
            }

            // Se calcula la edad a partir de la fecha de nacimiento
            var age = CalculateAge(user.BirthDate);

            if (age < 18)
            {
                throw new Exception("El usuario debe ser mayor de edad");
            }

            if (!IsValidEmail(user.Email))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            if (!IsValidPhone(user.PhoneNumber))
            {
                throw new Exception("El teléfono debe contener al menos 8 dígitos");
            }

            // Si ya viene hasheada desde una edición, no se vuelve a validar como texto plano.
            if (!IsHashedPassword(user.Password) && !IsValidPassword(user.Password))
            {
                throw new Exception("La contraseña debe tener al menos 8 caracteres, " +
                    "una mayúscula, una minúscula, un número y un carácter especial"
                );
            }

            if (!IsValidRole(user.Role))
            {
                throw new Exception("El rol debe ser Administrator, Engineer o Distributor");
            }

            if (!IsValidStatus(user.Status))
            {
                throw new Exception("El estado debe ser Pending, Active, Inactive o Locked");
            }
        }

        private bool HasEmptyFields(User user)
        {
            return string.IsNullOrWhiteSpace(user.Identification) ||
                   string.IsNullOrWhiteSpace(user.FirstName) ||
                   string.IsNullOrWhiteSpace(user.FirstLastName) ||
                   string.IsNullOrWhiteSpace(user.PhoneNumber) ||
                   string.IsNullOrWhiteSpace(user.Email) ||
                   string.IsNullOrWhiteSpace(user.Password) ||
                   string.IsNullOrWhiteSpace(user.Role) ||
                   string.IsNullOrWhiteSpace(user.Status);
        }

        // Validación de fecha de nacimiento: no puede ser una fecha futura
        private bool IsValidBirthDate(DateTime birthDate)
        {
            return birthDate.Date <= DateTime.Today;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
            );
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            return Regex.IsMatch(phone, @"^\d{8,}$");
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            return Regex.IsMatch(
                password,
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$"
            );
        }

        private bool IsValidRole(string role)
        {
            return role == "Administrator" ||
                   role == "Engineer" ||
                   role == "Distributor";
        }

        private bool IsValidStatus(string status)
        {
            return status == "Pending" ||
                   status == "Active" ||
                   status == "Inactive" ||
                   status == "Locked";
        }

        private bool IsHashedPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var parts = password.Split('.');
            return parts.Length == 3 && int.TryParse(parts[0], out _);
        }

        private void ValidateUserForUpdate(User user, User currentUser)
        {
            if (HasEmptyFields(user))
            {
                throw new Exception("Todos los campos obligatorios son requeridos");
            }

            if (!IsValidBirthDate(user.BirthDate))
            {
                throw new Exception("La fecha de nacimiento no es válida");
            }

            var age = CalculateAge(user.BirthDate);
            if (age < 18)
            {
                throw new Exception("El usuario debe ser mayor de edad");
            }

            if (!IsValidEmail(user.Email))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            if (!IsValidPhone(user.PhoneNumber))
            {
                throw new Exception("El teléfono debe contener al menos 8 dígitos");
            }

            if (!string.IsNullOrWhiteSpace(user.Password) &&
                !IsHashedPassword(user.Password) &&
                !string.Equals(user.Password, currentUser.Password, StringComparison.Ordinal) &&
                !IsValidPassword(user.Password))
            {
                throw new Exception("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial");
            }

            if (!IsValidRole(user.Role))
            {
                throw new Exception("El rol debe ser Administrator, Engineer o Distributor");
            }

            if (!IsValidStatus(user.Status))
            {
                throw new Exception("El estado debe ser Pending, Active, Inactive o Locked");
            }
        }

        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}