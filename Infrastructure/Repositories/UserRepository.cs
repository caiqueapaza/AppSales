using APISales.Context;
using APISales.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace APISales.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) 
        { 
            _context = context;
        }


        string RemoveAccents(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        string GenerateUserName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("First name and last name are required.");

            firstName = RemoveAccents(firstName);
            lastName = RemoveAccents(lastName);

            var normalizedFirstName = firstName.Trim().ToLower();

            var lastNameParts = lastName.Trim().ToLower().Split(' ');

            var ignoredWords = new[] { "da", "de", "do", "dos", "das" };

            var finalLastName = lastNameParts
                .LastOrDefault(p => !ignoredWords.Contains(p)) ?? lastNameParts.Last();

            return $"{normalizedFirstName}.{finalLastName}";
        }

        public IEnumerable<User> GetUsers()
        {
            return _context.Users.ToList();
        }
        public User GetUser(Guid id)
        {
            return _context.Users.Find(id);
        }

        public User GetLogin(string userName, string password)
        {
            var normalizedUserName = (userName ?? string.Empty).Trim().ToLower();
            var user = _context.Users.FirstOrDefault(x => x.UserName != null && x.UserName.ToLower() == normalizedUserName);

            if (user == null)
                return null;

            if (!user.IsActive)
                return null;

            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

            if (!passwordValid)
                return null;

            return user;
        }
        public User Create(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            var username = GenerateUserName(user.Name, user.LastName);

            var count = _context.Users.Count(u => u.UserName.StartsWith(username));

            if (count > 0)
            {
                username = $"{username}{count}";
            }

            user.UserName = username;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }
        public User Update(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.UpdatedAt = DateTime.UtcNow;
            if (!user.Password.StartsWith("$2"))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }

            _context.Entry(user).State = EntityState.Modified;
            _context.Entry(user).Property(x => x.CreatedAt).IsModified = false;
            _context.SaveChanges();

            return user;
        }
        public User Delete(Guid id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _context.Users.Remove(user);
            _context.SaveChanges();

            return user;
        }

    }
}
