using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace CoreApp
{
    // Lógica de negocio de usuarios: registro, acceso y mantenimiento.
    public class UserManager
    {
        // Cuentas estáticas para pruebas rápidas dentro de la app.
        private static readonly Dictionary<string, (string Password, string Role, string FirstName, string LastName, int Id)> StaticLoginAccounts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["admin@segede.local"] = ("Admin123!", "Admin", "Administrador", "Sistema", -1001),
            ["engineer@segede.local"] = ("Eng123!", "Engineer", "Ingeniero", "Sistema", -1002),
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

            ValidateUser(user); // Reviso datos obligatorios y formatos antes de guardar.

            var uCrud = new UserCrudFactory();

            // Evito que se repita el correo.
            var userByEmail = uCrud.RetrieveByEmail(user.Email);

            if (userByEmail != null)
            {
                throw new Exception("Ya existe un usuario registrado con ese correo electrónico");
            }

            // Evito que se repita la identificación.
            var userByIdentification = uCrud.RetrieveByIdentification(user.Identification);

            if (userByIdentification != null)
            {
                throw new Exception("Ya existe un usuario registrado con esa identificación");
            }

            // La edad se calcula desde la fecha de nacimiento.
            user.Age = CalculateAge(user.BirthDate);

            // La contraseña se guarda cifrada por seguridad.
            user.Password = HashPassword(user.Password);

            // Dejo valores iniciales para que la cuenta quede lista.
            user.Status = "Pending";
            user.FailedLoginAttempts = 0;
            user.LockoutEndAt = null;
            user.LastLoginAt = null;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = null;

            uCrud.Create(user);

            // Luego de guardar, genero el OTP para activar la cuenta.
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

            // Conservo la contraseña original para no volver a cifrarla por accidente.
            var originalPassword = user.Password;

            // Si no mandan una nueva contraseña, dejo la actual.
            user.Password = string.IsNullOrWhiteSpace(originalPassword)
                ? currentUser.Password
                : originalPassword;

            ValidateUserForUpdate(user, currentUser);

            // Recalculo la edad por si cambió la fecha de nacimiento.
            user.Age = CalculateAge(user.BirthDate);

            // Si llega una nueva contraseña, la vuelvo a cifrar solo una vez.
            user.Password = string.IsNullOrWhiteSpace(originalPassword)
                ? currentUser.Password
                : (IsHashedPassword(originalPassword) ? originalPassword : HashPassword(originalPassword));

            // Mantengo la fecha original de creación.
            user.CreatedAt = currentUser.CreatedAt;
            // Actualizo la fecha de modificación.
            user.UpdatedAt = DateTime.Now;

            uCrud.Update(user);
        }

        // Actualizar perfil desde perfil
        public User UpdateProfile(int userId, string firstName, string firstLastName, string? secondLastName, string phoneNumber)
        {
            if (userId <= 0)
            {
                throw new Exception("Sesión inválida. Inicie sesión nuevamente");
            }

            var uCrud = new UserCrudFactory();
            var user = uCrud.RetrieveById<User>(userId);

            if (user == null)
            {
                throw new Exception("No se encontró el usuario de la sesión actual");
            }

            user.FirstName = firstName?.Trim() ?? string.Empty;
            user.FirstLastName = firstLastName?.Trim() ?? string.Empty;
            user.SecondLastName = string.IsNullOrWhiteSpace(secondLastName) ? null : secondLastName.Trim();
            user.PhoneNumber = phoneNumber?.Trim() ?? string.Empty;

            ValidateUserForUpdate(user, user);
            user.Age = CalculateAge(user.BirthDate);
            user.UpdatedAt = DateTime.Now;
            uCrud.Update(user);

            return user;
        }

        // Solicita el cambio de correo: valida que el nuevo correo esté disponible y envía un OTP,
        // pero no modifica el correo del usuario hasta que el código sea confirmado.
        public void RequestEmailChange(int userId, string newEmail)
        {
            if (userId <= 0)
            {
                throw new Exception("Sesión inválida. Inicie sesión nuevamente");
            }

            newEmail = newEmail?.Trim() ?? string.Empty;
            if (!IsValidEmail(newEmail))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            var uCrud = new UserCrudFactory();
            var user = uCrud.RetrieveById<User>(userId);
            if (user == null)
            {
                throw new Exception("No se encontró el usuario de la sesión actual");
            }

            if (string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var existingUser = uCrud.RetrieveByEmail(newEmail);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new Exception("Ya existe otro usuario registrado con ese correo electrónico");
            }

            new OtpManager().GenerateAndSendOtp(newEmail, user.FirstName, "CHANGE_EMAIL");
        }

        // Confirma el cambio de correo validando el OTP enviado a la nueva dirección.
        // Si el código es válido y el correo sigue disponible, guarda el nuevo correo del usuario.
        public User ConfirmEmailChange(int userId, string newEmail, string otpCode)
        {
            if (userId <= 0)
            {
                throw new Exception("Sesión inválida. Inicie sesión nuevamente");
            }

            newEmail = newEmail?.Trim() ?? string.Empty;
            if (!IsValidEmail(newEmail))
            {
                throw new Exception("El correo electrónico no tiene un formato válido");
            }

            new OtpManager().ValidateOtp(newEmail, otpCode, "CHANGE_EMAIL");

            var uCrud = new UserCrudFactory();
            var user = uCrud.RetrieveById<User>(userId);
            if (user == null)
            {
                throw new Exception("No se encontró el usuario de la sesión actual");
            }

            var existingUser = uCrud.RetrieveByEmail(newEmail);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new Exception("Ya existe otro usuario registrado con ese correo electrónico");
            }

            user.Email = newEmail;
            user.UpdatedAt = DateTime.Now;
            uCrud.Update(user);

            return user;
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
                var remainingMinutes = (int)Math.Ceiling((user.LockoutEndAt.Value - DateTime.Now).TotalMinutes);
                throw new Exception($"La cuenta se encuentra bloqueada temporalmente. Intente nuevamente en {remainingMinutes} minutos.");
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

                // Regla de seguridad: se cuentan los intentos y al 3er fallo se bloquea 15 minutos
                if (user.FailedLoginAttempts >= 3)
                {
                    user.LockoutEndAt = DateTime.Now.AddMinutes(15);
                    uCrud.UpdateLoginAttempts(user);

                    // Enviar notificación de bloqueo de cuenta
                    try
                    {
                        var notificationManager = new NotificationManager();
                        notificationManager.SendAccountLockedNotification(
                            user.Email,
                            user.FirstName,
                            15  // minutos de bloqueo
                        );
                    }
                    catch
                    {
                        // No bloquear la operación si falla el envío del correo
                    }

                    throw new Exception("3 intentos agotados. La cuenta se bloqueó por 15 minutos.");
                }

                uCrud.UpdateLoginAttempts(user);

                var remainingAttempts = 3 - user.FailedLoginAttempts;
                throw new Exception($"La contraseña es incorrecta. Le quedan {remainingAttempts} intento{(remainingAttempts == 1 ? string.Empty : "s")}. ");
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

            // Las cuentas estáticas de demostración no aplican bloqueo temporal por intentos.
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

        // Termina el inicio de sesión cuando el OTP ya fue validado.
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

            // Enviar notificación de inicio de sesión exitoso
            try
            {
                var notificationManager = new NotificationManager();
                notificationManager.SendLoginSuccessNotification(
                    user.Email,
                    user.FirstName,
                    DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                );
            }
            catch
            {
                // No bloquear el login por un fallo en el envío del correo
            }

            return user;
        }

        // Pide cambio de contraseña sin sesión y manda el OTP al correo.
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

        // Cambio de contraseña con sesión activa. Aquí solo se valida la contraseña actual.
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

        // Confirma el cambio de contraseña con OTP y actualiza la clave.
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

        // Solicita el restablecimiento de contraseña y envía el código OTP.
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

        // Confirma el restablecimiento después de revisar el OTP.
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

        // Activa la cuenta con OTP y deja la contraseña definida por el usuario.
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

        // Guarda la contraseña con PBKDF2, SHA256 y un salt aleatorio.
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

        // Compara la contraseña guardada con la que escribe el usuario.
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

        // Validaciones internas del usuario.

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

        // La fecha de nacimiento no puede ser futura.
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
                   role == "Buyer" ||
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
                throw new Exception("El rol debe ser Administrator, Buyer, Engineer o Distributor");
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