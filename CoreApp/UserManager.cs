using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace CoreApp
{
    public class UserManager
    {
        public List<User> RetrieveAllUsers()
        {
            var uCrud = new UserCrudFactory();

            return uCrud.RetrieveAll<User>();
        }

        public void Create(User u)
        {
            if (HasEmptyFields(u))
            {
                throw new Exception("Todos los campos obligatorios son requeridos");
            }

            if (!IsBirthDateValid(u))
            {
                throw new Exception("La fecha de nacimiento no puede ser futura");
            }

            if (u.Age < 18)
            {
                throw new Exception("El usuario debe ser mayor de edad");
            }

            if (!IsValidStatus(u))
            {
                throw new Exception("El estado debe ser AC o IN");
            }

            if (!IsValidEmail(u.Email))
            {
                throw new Exception("El correo no tiene un formato válido");
            }

            if (!IsValidPhone(u.PhoneNumber))
            {
                throw new Exception("El teléfono debe tener al menos 8 dígitos");
            }

            var uCrud = new UserCrudFactory();

            uCrud.Create(u);
        }

        public void Update(User u)
        {
            var uCrud = new UserCrudFactory();

            uCrud.Update(u);
        }

        public void Delete(User u)
        {
            var uCrud = new UserCrudFactory();

            uCrud.Delete(u);
        }

        // Validaciones

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

        private bool IsBirthDateValid(User user)
        {
            return user.BirthDate <= DateTime.Now;
        }

        private bool IsValidStatus(User user)
        {
            return user.Status == "Activo" ||
                   user.Status == "Inactivo";
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email)
                && email.Contains("@")
                && email.Contains(".");
        }

        private bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) &&
                   phone.Length >= 8;
        }
    }
}